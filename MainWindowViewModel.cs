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
        private MediaPlayer _mediaPlayer;
        private string _selectedMonitor;

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
            _mediaPlayer = new MediaPlayer(_libVLC);

            _screenObjects = _window.Screens.All.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            SelectedMonitor = Monitors.FirstOrDefault();

            OpenFileCommand = new AsyncRelayCommand(OpenFile);
        }

        public ICommand OpenFileCommand { get; }

        private async Task OpenFile() {
            var dialog = new OpenFileDialog { AllowMultiple = false };
            var result = await dialog.ShowAsync(_window);
            if (result != null && result.Length > 0) {
                var media = new Media(_libVLC, new Uri(result[0]));

                // Position the video window on the selected monitor
                var videoWindow = new VideoWindow(_libVLC, media);
                var selectedScreenIndex = Monitors.IndexOf(SelectedMonitor);
                var selectedScreen = _screenObjects[selectedScreenIndex];
                videoWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                videoWindow.Position = selectedScreen.Bounds.TopLeft;
                videoWindow.Show();
                videoWindow.WindowState = WindowState.FullScreen;
            }
        }

    }
}
