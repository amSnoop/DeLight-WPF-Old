using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using LibVLCSharp.Shared;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Platform;

namespace DeLightNew {
    public class MainWindowViewModel : ObservableObject {
        private readonly Window _window;
        private LibVLC _libVLC;
        private string _selectedMonitor;
        private VideoWindow _videoWindow;

        private int _volume = 20;

        public int Volume
        {
            get => _volume;
            set
            {
                SetProperty(ref _volume, value);
                _videoWindow.VideoViewControl.MediaPlayer.Volume = value;
            }
        }

        public bool VideoIsVisible => _videoWindow.IsVisible;

        public List<string> Monitors { get; }
        private List<Screen> _screenObjects;

        public string SelectedMonitor {
            get => _selectedMonitor;
            set => SetProperty(ref _selectedMonitor, value);
        }

        public MainWindowViewModel(Window window) {
            _window = window;
            Core.Initialize();
            _libVLC = new LibVLC();
            _videoWindow = new(_libVLC);
            _screenObjects = _window.Screens.All.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            SelectedMonitor = Monitors.FirstOrDefault() ?? "";

            OpenFileCommand = new AsyncRelayCommand(OpenFile);
        }

        public void CloseVideoPlayback()
        {
            _videoWindow.Close();
        }
        public void HideVideoPlayback()
        {
            _videoWindow.Hide();
        }

        public ICommand OpenFileCommand { get; }

        private async Task OpenFile() {
            var dialog = new OpenFileDialog { AllowMultiple = false };
            var result = await dialog.ShowAsync(_window);
            if (result != null && result.Length > 0) {
                var media = new Media(_libVLC, new Uri(result[0]));
                
                _videoWindow.Media = media;
                var selectedScreenIndex = Monitors.IndexOf(SelectedMonitor);
                var selectedScreen = _screenObjects[selectedScreenIndex];
                _videoWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                _videoWindow.Position = selectedScreen.Bounds.TopLeft;
                if(_videoWindow.IsVisible)
                    _videoWindow.Play(Volume);
                else
                    _videoWindow.Show();
                _videoWindow.WindowState = WindowState.FullScreen;
                _window.Focus();
            }
        }

    }
}
