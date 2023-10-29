using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private readonly DispatcherTimer _timer = new();
        private int _timerInterval = 50;
        private int _totalTicks = 0;
        private bool foundDuration = false;


        #region Observable Properties

        [ObservableProperty]
        private string selectedMonitor = "";
        [ObservableProperty]
        private List<string> monitors = new();
        [ObservableProperty]
        private CueViewModel previewCueViewModel = new();
        [ObservableProperty]
        private CueViewModel activeCueViewModel = new();
        [ObservableProperty]
        private ShowRunner showRunner;
        [ObservableProperty]
        private double subTitleFontFactor = 1.2;
        [ObservableProperty]
        private double titleFontFactor = 3;
        [ObservableProperty]
        private double cueFontFactor = 1.2;

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

        public double BodyFontSize
        {
            get
            {
                double baseFontSize = 12;
                double scaleFactor = 1 + Math.Log(_window.ActualHeight / 720.0);
                double maxFontSize = 25;
                double fontSize = Math.Max(Math.Min(baseFontSize * scaleFactor, maxFontSize), baseFontSize);
                return fontSize;
            }
        }

        public double CueFontSize => BodyFontSize * CueFontFactor;
        public double SubtitleFontSize => BodyFontSize * SubTitleFontFactor;

        public double TitleFontSize => BodyFontSize * TitleFontFactor;

        public void UpdateWindowSize()
        {
            OnPropertyChanged(nameof(TitleFontSize));
            OnPropertyChanged(nameof(SubtitleFontSize));
            OnPropertyChanged(nameof(BodyFontSize));
            OnPropertyChanged(nameof(CueFontSize));
            OnPropertyChanged(nameof(Padding));
        }

        #endregion

        public MainWindowViewModel(MainWindow window)
        {
            showRunner = new(Show.Load(GlobalSettings.Instance.LastShowPath), VideoWindow);
            showRunner.PropertyChanged += ActiveCueChanged;
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
            _window.CueList.SelectionChanged += CueList_SelectionChanged;
        }

        private void CueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_window.CueEditorWindow.DataContext is CueEditorViewModel vm && !vm.IsSaved)
            {
                var result = System.Windows.MessageBox.Show("You have unsaved changes. Would you like to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if(result == MessageBoxResult.Yes)
                {
                    vm.Save(); 
                }
                else if(result == MessageBoxResult.Cancel)
                {
                    _window.CueList.SelectedItem = e.RemovedItems[0];
                    return;
                }
            }
            if(ShowRunner.SelectedCue != null)
                _window.CueEditorWindow.DataContext = new CueEditorViewModel(ShowRunner.SelectedCue);
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
            ShowRunner.Stop();
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
            ShowRunner.Stop();
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
