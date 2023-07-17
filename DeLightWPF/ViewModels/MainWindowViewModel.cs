using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Models;
using DeLightWPF.Utilities;
using DeLightWPF.ViewModels;

namespace DeLightWPF {
    public partial class MainWindowViewModel : ObservableObject {
        private readonly MainWindow _window;
        private string _selectedMonitor = "";
        private VideoWindow VideoWindow { get; set; } = new();
        private readonly List<Screen> _screenObjects;

        [ObservableProperty]
        private CueViewModel previewCueViewModel = new();

        [ObservableProperty]
        private CueViewModel activeCueViewModel = new();

        [ObservableProperty]
        private Show show;

        [ObservableProperty]
        private int factor = 35;
        [ObservableProperty]
        private int factor1 = 40;
        [ObservableProperty]
        private int factor2 = 30;
        [ObservableProperty]
        private int factor3 = 40;
        partial void OnFactorChanged(int value) {
            UpdateFonts();
        }
        partial void OnFactor1Changed(int value) {
            UpdateFonts();
        }
        partial void OnFactor2Changed(int value) {
            UpdateFonts();
        }
        partial void OnFactor3Changed(int value) {
            UpdateFonts();
        }
        public void UpdateFonts() {
            OnPropertyChanged(nameof(TitleFontSize));
            OnPropertyChanged(nameof(SubtitleFontSize));
            OnPropertyChanged(nameof(BodyFontSize));
            OnPropertyChanged(nameof(CueFontSize));
        }
        public double TitleFontSize {
            get
            {
                double baseFontSize = 30;
                double scaleFactor = 1 + Math.Log(_window.Width / 1000.0);
                double maxFontSize = 50;
                double fontSize = Math.Max(Math.Min(baseFontSize * scaleFactor, maxFontSize), baseFontSize);

                return fontSize;
            }
        }

        public int RowHeight {
            get
            {
                double windowHeight = _window.Height;
                double rowHeight = windowHeight / 30;
                return (int)rowHeight;
            }
        }

        public void UpdatePadding() {
            OnPropertyChanged(nameof(Padding));
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
                if (SetProperty(ref activeCue, value)) {
                    // Notify the detail view model to update
                    ActiveCueViewModel.CurrentCue = value;
                    VideoWindow.Load(value);
                }
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
        public List<string> Monitors { get; }
        public string SelectedMonitor {
            get => _selectedMonitor;
            set => SetProperty(ref _selectedMonitor, value);
        }



        public MainWindowViewModel(MainWindow window) {
            show = Show.Load(GlobalSettings.Instance.LastShowPath);
            _window = window;
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            SelectedMonitor = Monitors.FirstOrDefault() ?? "";

        }
        public void HideVideoPlayback() {
            VideoWindow.Stop();
            VideoWindow.Hide();
        }
        public void Play() {
            VideoWindow?.Play();
            VideoWindow?.Show();
            _window.Activate();
        }
        public void Stop() {
            VideoWindow.Stop();
            _window.Activate();

        }
    }
}
