using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using System.Windows;

namespace DeLightNew {
    public class MainWindowViewModel : ObservableObject {
        private readonly Avalonia.Controls.Window _window;
        private LibVLC _libVLC;
        private string _selectedMonitor;
        private NewVidWindow _videoWindow;

        private int _volume = 20;

        public int Volume
        {
            get => _volume;
            set
            {
                SetProperty(ref _volume, value);
                _videoWindow.mediaElement.Volume = value;
            }
        }

        public bool VideoIsVisible => _videoWindow.IsVisible;

        public List<string> Monitors { get; }
        private List<Screen> _screenObjects;

        public string SelectedMonitor {
            get => _selectedMonitor;
            set => SetProperty(ref _selectedMonitor, value);
        }

        public MainWindowViewModel(Avalonia.Controls.Window window) {
            _window = window;
            Core.Initialize();
            _libVLC = new LibVLC();
            _videoWindow = new();
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
            var dialog = new Avalonia.Controls.OpenFileDialog { AllowMultiple = false };
            var result = await dialog.ShowAsync(_window);
            if (result != null && result.Length > 0) {
                var media = new Uri(result[0]);
                
                _videoWindow.mediaElement.Source = media;
                var selectedScreenIndex = Monitors.IndexOf(SelectedMonitor);
                var selectedScreen = _screenObjects[selectedScreenIndex];
                if (!_videoWindow.IsVisible)
                    _videoWindow.Show();

                _videoWindow.mediaElement.Play();
                _videoWindow.WindowState = WindowState.Maximized;
                _window.Focus();
            }
        }

    }
}
