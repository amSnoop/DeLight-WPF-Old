using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Models;
using DeLightWPF.Utilities;
using DeLightWPF.ViewModels;
using Microsoft.Win32;

namespace DeLightWPF {
    public partial class MainWindowViewModel : ObservableObject {
        private readonly MainWindow _window;

        private List<Screen> _screenObjects;
        [ObservableProperty]
        private string selectedMonitor = "";
        [ObservableProperty]
        private List<string> monitors = new();

        [ObservableProperty]
        private double masterVol = 2.5;

        private VideoWindow VideoWindow { get; set; } = new();

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






        public void UpdateWindowSize() {
            OnPropertyChanged(nameof(TitleFontSize));
            OnPropertyChanged(nameof(SubtitleFontSize));
            OnPropertyChanged(nameof(BodyFontSize));
            OnPropertyChanged(nameof(CueFontSize));
            OnPropertyChanged(nameof(Padding));
        }
        public double TitleFontSize {
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

        #region Non [ObservableProperty] Properties that send updates to the view

        private Cue? selectedCue;

        private Cue? activeCue;

        public Cue? ActiveCue {
            get => activeCue;
            set
            {
                if (activeCue is not null) {
                    activeCue.IsActive = false;
                    activeCue.PropertyChanged -= ActiveCue_PropertyChanged;
                }
                if (SetProperty(ref activeCue, value)) {
                    if (activeCue is not null) {
                        activeCue.IsActive = true;
                        activeCue.PropertyChanged += ActiveCue_PropertyChanged;
                    }
                    // Notify the detail view model to update
                    ActiveCueViewModel.CurrentCue = value;
                };
            }
        }

        private void ActiveCue_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Cue.Volume)) {
                VideoWindow.SetVolume(ActiveCue?.Volume ?? 0);
            }
        }

        public Cue? SelectedCue {
            get => selectedCue;
            set
            {
                if (SetProperty(ref selectedCue, value)) {
                    // Notify the detail view model to update
                    PreviewCueViewModel.CurrentCue = value;
                }
            }
        }




        #endregion


        public bool VideoIsVisible => VideoWindow.IsVisible;


        partial void OnSelectedMonitorChanged(string value) {
            var screen = _screenObjects[Monitors.IndexOf(SelectedMonitor)];
            VideoWindow.SetScreen(screen);
        }


        public MainWindowViewModel(MainWindow window) {
            show = Show.Load(GlobalSettings.Instance.LastShowPath);
            _window = window;
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            if (Screen.PrimaryScreen == null) {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
                VideoWindow.SetScreen(_screenObjects.First());
            }
            else {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(Screen.PrimaryScreen)];
                VideoWindow.SetScreen(Screen.PrimaryScreen);
            }
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            _window.DefaultCueDisplay.DataContext = GlobalSettings.Instance.DefaultCue;
            _window.SettingsDisplay.DataContext = GlobalSettings.Instance;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e) {
            var screen = _screenObjects[_window.MonitorSelector.SelectedIndex];
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            try {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(screen)];
            }
            catch (Exception) {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
            }
        }
        public void HideVideoPlayback() {
            VideoWindow.Hide();
        }

        public void PlayNextCue() {
            // The show is over and no cue is selected
            if (ShowComplete && SelectedCue == null)
                return;

            // The show was over, but a new cue is manually selected
            ShowComplete = ShowComplete && SelectedCue == null;

            SelectedCue ??= Show.Cues.FirstOrDefault();

            if (SelectedCue != null) {
                ActiveCue = SelectedCue;
                int index = Show.Cues.IndexOf(SelectedCue);

                SelectedCue = index < Show.Cues.Count - 1 ? Show.Cues[index + 1] : null;
                ShowComplete = SelectedCue == null;

                Play();
            }
        }

        public void Play() {
            VideoWindow.Show();
            VideoWindow.Play(ActiveCue);
            _window.Activate();
        }
        public void Stop() {
            VideoWindow.Stop();
            _window.Activate();

        }
    }
}
