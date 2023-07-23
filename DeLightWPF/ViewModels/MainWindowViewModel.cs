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
        private ShowRunner showRunner;
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

        #endregion

        public MainWindowViewModel(MainWindow window)
        {
            ShowRunner = new(Show.Load(GlobalSettings.Instance.LastShowPath), VideoWindow);
            ShowRunner.PropertyChanged += ActiveCueChanged;
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
        }

        #region Other Event Listeners

        private void ActiveCueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ShowRunner.ActiveCue))
            {
                ActiveCueViewModel.CurrentCue = ShowRunner.ActiveCue;
            }
            if(e.PropertyName == nameof(ShowRunner.SelectedCue))
            {
                PreviewCueViewModel.CurrentCue = ShowRunner.SelectedCue;
            }
        }

        //Volume property of active cue changed, send it to video window.
        private void ActiveCue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cue.Volume))
            {
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
            ShowRunner.Play();
        }

        public void Play()
        {
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
        }
        public void SoftStop()
        {
            _timer.Stop();
        }


    }
}
