using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Models;
using DeLightWPF.Utilities;
using DeLightWPF.ViewModels;
using Microsoft.Win32;

namespace DeLightWPF
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly MainWindow _window;
        private List<Screen> _screenObjects;

        public bool VideoIsVisible => VideoWindow.IsVisible;
        public VideoWindow VideoWindow { get; set; } = new();

        private DispatcherTimer _timer = new();
        private int _timerInterval = 50;
        private int _totalTicks = 0;
        private bool foundDuration = false;
        #region Observable Properties

        [ObservableProperty]
        private string selectedMonitor = "";
        [ObservableProperty]
        private List<string> monitors = new();
        [ObservableProperty]
        private double masterVol = 2.5;
        [ObservableProperty]
        private CueViewModel previewCueViewModel = new();
        [ObservableProperty]
        private CueViewModel activeCueViewModel = new();
        [ObservableProperty]
        private Show show;
        [ObservableProperty]
        private bool showComplete = false;
        [ObservableProperty]
        private int factor = 35;
        [ObservableProperty]
        private int factor1 = 40;
        [ObservableProperty]
        private int factor2 = 35;
        [ObservableProperty]
        private int factor3 = 40;

        #endregion


        public Dictionary<FadeType, string> FadeTypeStrings { get; } = new() {
            { FadeType.ShowXPress, "ShowXPress" },
            { FadeType.FadeOver, "Fade Over" }
        };
        public Dictionary<EndAction, string> EndActionStrings { get; } = new() {
            { EndAction.Loop, "Loop" },
            { EndAction.FadeAfterEnd, "Fade After End" },
            { EndAction.FadeBeforeEnd, "Fade Before End" },
            { EndAction.Freeze, "Freeze" }
        };

        public Dictionary<string, int> ImportantTimes { get; set; } = new() {
            { "vid", 1500 },
            { "lights", 1500 }
        };




        #region Font Size Properties

        public double TitleFontSize
        {
            get
            {
                double baseFontSize = 30;
                double scaleFactor = 1 + Math.Log(_window.ActualWidth / 1000.0);
                double maxFontSize = 50;
                double fontSize = Math.Max(Math.Min(baseFontSize * scaleFactor, maxFontSize), baseFontSize);

                return fontSize;
            }
        }

        public double CueFontSize => TitleFontSize * Factor3 / 100;
        public double SubtitleFontSize => TitleFontSize * Factor1 / 100;

        public double BodyFontSize => TitleFontSize * Factor2 / 100;

        public void UpdateWindowSize()
        {
            OnPropertyChanged(nameof(TitleFontSize));
            OnPropertyChanged(nameof(SubtitleFontSize));
            OnPropertyChanged(nameof(BodyFontSize));
            OnPropertyChanged(nameof(CueFontSize));
            OnPropertyChanged(nameof(Padding));
        }

        #endregion

        #region Non [ObservableProperty] Properties that send updates to the view

        private Cue? selectedCue;

        private Cue? activeCue;

        public Cue? ActiveCue
        {
            get => activeCue;
            set
            {
                if (activeCue is not null && activeCue != value)
                {
                    activeCue.IsActive = false;
                    activeCue.PropertyChanged -= ActiveCue_PropertyChanged;
                }
                if (SetProperty(ref activeCue, value))
                {
                    if (activeCue is not null)
                    {
                        activeCue.IsActive = true;
                        activeCue.PropertyChanged += ActiveCue_PropertyChanged;
                    }
                    // Notify the detail view model to update
                    ActiveCueViewModel.CurrentCue = value;
                };
            }
        }

        public Cue? SelectedCue
        {
            get => selectedCue;
            set
            {
                if (SetProperty(ref selectedCue, value))
                {
                    // Notify the detail view model to update
                    PreviewCueViewModel.CurrentCue = value;
                }
            }
        }

        #endregion

        public MainWindowViewModel(MainWindow window)
        {
            show = Show.Load(GlobalSettings.Instance.LastShowPath);
            _window = window;
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            if (Screen.PrimaryScreen == null)
            {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
                VideoWindow.SetScreen(_screenObjects.First());
            }
            else
            {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(Screen.PrimaryScreen)];
                VideoWindow.SetScreen(Screen.PrimaryScreen);
            }
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            _window.DefaultCueDisplay.DataContext = GlobalSettings.Instance.DefaultCue;
            _window.SettingsDisplay.DataContext = GlobalSettings.Instance;
            VideoWindow.GotFocus += VideoWindow_GotFocus;
            _timer.Interval = TimeSpan.FromMilliseconds(_timerInterval);
            _timer.Tick += Timer_Tick;
        }

        #region Other Event Listeners

        //Volume property of active cue changed, send it to video window.
        private void ActiveCue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cue.Volume))
            {
                VideoWindow.SetVolume(ActiveCue?.Volume ?? 0);
            }
        }

        //Monitor selection changed, send it to video window.
        partial void OnSelectedMonitorChanged(string value)
        {
            var screen = _screenObjects[Monitors.IndexOf(SelectedMonitor)];
            VideoWindow.SetScreen(screen);
        }

        //refocus the main window if the video window is clicked [Not working]
        public void VideoWindow_GotFocus(object sender, EventArgs e)
        {
            _window.Activate();
            _window.Focus();
        }

        //A new monitor is plugged in or display settings are changed, update the list of monitors
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            var screen = _screenObjects[_window.MonitorSelector.SelectedIndex];
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            try
            {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(screen)];
            }
            catch (Exception)
            {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
            }
        }


        #endregion


        public void HideVideoPlayback()
        {
            VideoWindow.Hide();
        }

        public void PlayNextCue()
        {
            // The show is over and no cue is selected
            if (ShowComplete && SelectedCue == null)
                return;

            // The show was over, but a new cue is manually selected
            ShowComplete = ShowComplete && SelectedCue == null;

            SelectedCue ??= Show.Cues.FirstOrDefault();

            if (SelectedCue != null)
            {
                ActiveCue = SelectedCue;
                int index = Show.Cues.IndexOf(SelectedCue);

                SelectedCue = index < Show.Cues.Count - 1 ? Show.Cues[index + 1] : null;
                ShowComplete = SelectedCue == null;

                Play();
            }
        }

        public void Play()
        {
            VideoWindow.Show();
            VideoWindow.Play(ActiveCue);
            _totalTicks = 0;
            _timer.Start();
            _window.Activate();
        }
        public void Stop()
        {
            VideoWindow.Stop();
            _timer.Stop();
            _window.Activate();
        }
        public void SoftPlay()
        {
            _timer.Start();
            VideoWindow.SoftPlay();
        }
        public void SoftStop()
        {
            _timer.Stop();
            VideoWindow.SoftStop();
        }

        public void CalculateImportantTimes()
        {

            if (ActiveCue != null)
            {
                var vidEndTime = VideoWindow.GetUpdates().Duration.TotalSeconds;
                //pick a random number for light end time between 0 and 10 seconds
                double lightEndTime = Math.Round(new Random().NextDouble() * 10, 1);

                ImportantTimes["vid"] = 0;
                ImportantTimes["lights"] = 0;
                var cueTime = FindRealCueDuration(vidEndTime, lightEndTime);
                ActiveCueViewModel.RealDuration = cueTime;
                if (vidEndTime < cueTime)
                {
                    if (ActiveCue.VidEndAction == EndAction.FadeAfterEnd || ActiveCue.VidEndAction == EndAction.Loop)
                        ImportantTimes["vid"] = (int)(vidEndTime * _timerInterval);
                    else if (ActiveCue.VidEndAction == EndAction.FadeBeforeEnd)
                        ImportantTimes["vid"] = Math.Max((int)((vidEndTime - ActiveCue.FadeOutTime) * _timerInterval), 0);
                    else
                        ImportantTimes["vid"] = -1;
                }
                if (vidEndTime >= cueTime)
                {
                    if (ActiveCue.VidEndAction == EndAction.FadeBeforeEnd)
                        ImportantTimes["vid"] = Math.Max((int)((cueTime - ActiveCue.FadeOutTime) * _timerInterval), 0);
                    else if (ActiveCue.VidEndAction == EndAction.Freeze)
                        ImportantTimes["vid"] = -1;
                    else
                        ImportantTimes["vid"] = (int)(cueTime * _timerInterval);
                }


                if (lightEndTime < cueTime)
                {
                    if (ActiveCue.LightEndAction == EndAction.FadeAfterEnd || ActiveCue.LightEndAction == EndAction.Loop)
                        ImportantTimes["lights"] = (int)(lightEndTime * _timerInterval);
                    else if (ActiveCue.LightEndAction == EndAction.FadeBeforeEnd)
                        ImportantTimes["lights"] = Math.Max((int)((lightEndTime - ActiveCue.FadeOutTime) * _timerInterval), 0);
                    else
                        ImportantTimes["lights"] = -1;
                }
                if (lightEndTime >= cueTime)
                {

                    if (ActiveCue.LightEndAction == EndAction.FadeBeforeEnd)
                        ImportantTimes["lights"] = Math.Max((int)((cueTime - ActiveCue.FadeOutTime) * _timerInterval), 0);
                    else if (ActiveCue.LightEndAction == EndAction.Freeze)
                        ImportantTimes["lights"] = -1;
                    else
                        ImportantTimes["lights"] = (int)(cueTime * _timerInterval);
                }
            }
        }

        public double FindRealCueDuration(double vidEnd, double lightEnd)
        {
            if (ActiveCue == null)
                return 0;
            if (ActiveCue.Duration == 0)
            {
                if (vidEnd > lightEnd)
                    return vidEnd;
                else
                    return lightEnd;
            }
            else
                return ActiveCue.Duration;
        }

        //alright, here we go. Now comes the complicated ass timer shit....
        public void Timer_Tick(object? sender, EventArgs e)
        {
            if (ActiveCue != null)
            {
                var status = VideoWindow.GetUpdates();
                if (!status.Error && !foundDuration)
                {
                    CalculateImportantTimes();
                    foundDuration = true;
                }
                //send ticks to light here


                
                _totalTicks++;
            }

        }


    }
}
