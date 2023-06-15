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

namespace DeLightWPF {
    public partial class MainWindowViewModel : ObservableObject {
        private readonly MainWindow _window;
        private string _selectedMonitor = "";
        private VideoWindow _videoWindow;
        private VideoWindow? _newVideoWindow;
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
        

        public bool VideoIsVisible => _videoWindow.IsVisible;
        public List<string> Monitors { get; }
        public string SelectedMonitor {
            get => _selectedMonitor;
            set => SetProperty(ref _selectedMonitor, value);
        }



        public MainWindowViewModel(MainWindow window) {
            show = Show.Load(GlobalSettings.Instance.LastShowPath);
            _window = window;
            _videoWindow = new();
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            SelectedMonitor = Monitors.FirstOrDefault() ?? "";

            OpenFileCommand = new AsyncRelayCommand(OpenFile);
            _videoWindow.Activated += ReturnMainWindowFocus;
        }
        public void ReturnMainWindowFocus(object? sender, EventArgs e)
        {
            _window.Activate();
        }
        public void CloseVideoPlayback()
        {
            _videoWindow.Close();
        }
        public void HideVideoPlayback()
        {
            _videoWindow.HideWindow();
        }

        public ICommand OpenFileCommand { get; }

        private async Task OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Multiselect = false };
            var result = dialog.ShowDialog(_window);
            if (result.HasValue && result.Value)
            {
                _newVideoWindow = new VideoWindow
                {
                    MediaUri = new Uri(dialog.FileName)
                };
                var selectedScreenIndex = Monitors.IndexOf(SelectedMonitor);
                var selectedScreen = _screenObjects[selectedScreenIndex];
                _newVideoWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                _newVideoWindow.Top = selectedScreen.Bounds.Top;
                _newVideoWindow.Left = selectedScreen.Bounds.Left;
                _newVideoWindow.VideoViewControl.Volume = SelectedCue?.Volume ?? 0;
                _newVideoWindow.WindowStyle = WindowStyle.None;
                _newVideoWindow.Show();
                _window.Focus();
                _newVideoWindow.WindowState = WindowState.Maximized;
                _newVideoWindow.ShowInTaskbar = false;
                _newVideoWindow.Topmost = true;
                _newVideoWindow.ResizeMode = ResizeMode.NoResize;
                _newVideoWindow.Opacity = 0;

                _newVideoWindow.Play();

                await FadeInOut();

                _videoWindow?.Close();

                _videoWindow = _newVideoWindow;
                _newVideoWindow = null;
            }
        }

        private Task FadeInOut()
        {
            var duration = TimeSpan.FromSeconds(SelectedCue?.FadeInTime ?? 0);
            var fadeIn = new DoubleAnimation(0, 1, duration);
            //var fadeOutAudio = new DoubleAnimation(_window.VolumeSlider.Value, 0.0, duration);

            var tcs = new TaskCompletionSource<bool>();

            fadeIn.Completed += (s, e) =>
            {
                tcs.SetResult(true);
            };

            _newVideoWindow?.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            //_videoWindow?.VideoViewControl.BeginAnimation(MediaElement.VolumeProperty, fadeOutAudio);

            return tcs.Task;
        }
    }
}
