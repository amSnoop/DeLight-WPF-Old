using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Animation;
using DeLightWPF.Models;
using DeLightWPF.Utilities;
using DeLightWPF.ViewModels;
using System.Security.Cryptography;

namespace DeLightWPF {
    public partial class MainWindowViewModel : ObservableObject {
        private readonly MainWindow _window;
        private string _selectedMonitor = "";
        public CuePlayer CuePlayer { get; set; } = new();
        private readonly List<Screen> _screenObjects;


        [ObservableProperty]
        private CueViewModel previewCueViewModel = new();

        [ObservableProperty]
        private CueViewModel activeCueViewModel = new();

        [ObservableProperty]
        private Show show;


        #region Non [ObservableProperty] Properties that send updates to the view

        private Cue? selectedCue;

        private Cue? activeCue;

        public Cue? ActiveCue
        {
            get => activeCue;
            set
            {
                if (SetProperty(ref activeCue, value))
                {
                    // Notify the detail view model to update
                    ActiveCueViewModel.CurrentCue = value;
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
        

        public bool VideoIsVisible => CuePlayer.VideoWindow != null;
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
        public void HideVideoPlayback()
        {
            CuePlayer.HardStop();
        }
        public async void Play() {
            await CuePlayer.Play(ActiveCue);
            _window.Activate();
        }
        public async void Stop() {
            await CuePlayer.Stop(ActiveCue);
            _window.Activate();

        }
        public void OnActiveCueViewModelChanged() {
            CuePlayer.Load(ActiveCue);
        }
    }
}
