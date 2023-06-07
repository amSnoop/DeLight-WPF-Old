using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeLightWPF {
    public class MainWindowViewModel : ObservableObject {
        private readonly Window _window;
        private string _selectedMonitor;
        private VideoWindow _videoWindow;

        private int _volume = 20;

        public int Volume
        {
            get => _volume;
            set
            {
                SetProperty(ref _volume, value);
                _videoWindow.VideoViewControl.Volume = value;
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
            _videoWindow = new();
            _screenObjects = Screen.AllScreens.ToList();
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
            _videoWindow.HideWindow();
        }

        public ICommand OpenFileCommand { get; }

        private async Task OpenFile() {
            var dialog = new Microsoft.Win32.OpenFileDialog { Multiselect = false };
            var result = dialog.ShowDialog(_window);
            if (result.HasValue && result.Value) {
                _videoWindow.MediaUri = new Uri(dialog.FileName);
                var selectedScreenIndex = Monitors.IndexOf(SelectedMonitor);
                var selectedScreen = _screenObjects[selectedScreenIndex];
                _videoWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                _videoWindow.Top = selectedScreen.Bounds.Top;
                _videoWindow.Left = selectedScreen.Bounds.Left;
                if(_videoWindow.IsVisible)
                    _videoWindow.Play(Volume);
                else
                    _videoWindow.Show();
                _videoWindow.WindowState = WindowState.Maximized;
                _videoWindow.WindowStyle = WindowStyle.None;
                _videoWindow.ShowInTaskbar = false;
                _videoWindow.Topmost = true;
                _videoWindow.ResizeMode = ResizeMode.NoResize;
                _window.Focus();
            }
        }

    }
}
