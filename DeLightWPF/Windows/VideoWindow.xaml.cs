using System;
using System.Windows;
using System.Windows.Forms;

namespace DeLightWPF {
    public partial class VideoWindow : Window {
        public string? MediaUriString { get; set; }

        public VideoWindow(string? mediaUri, Screen? screen)
        {
            InitializeComponent();
            VideoViewControl.MediaOpened += OnMediaOpened;
            MediaUriString = mediaUri;
            if(mediaUri != null && mediaUri != "")
                VideoViewControl.Source = new(mediaUri);
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = screen?.Bounds.Top ?? Screen.PrimaryScreen?.Bounds.Top ?? 0;
            Left = screen?.Bounds.Left ?? Screen.PrimaryScreen?.Bounds.Top ?? 0;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ShowInTaskbar = false;
            Topmost = true;
            ResizeMode = ResizeMode.NoResize;
            Opacity = 0;
            VideoViewControl.Opacity = 0;
        }

        private void OnMediaOpened(object? sender, RoutedEventArgs e) {
            VideoViewControl.Pause();
        }
        public VideoWindow() {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e) {
            VideoViewControl.Pause();
            VideoViewControl.Stop();
            base.OnClosed(e);
        }

        public void HideWindow() {
            VideoViewControl.Pause();
            VideoViewControl.Stop();
            Hide();
        }

        public void Play() {
            VideoViewControl.Play();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
            e.Handled = true;
        }
    }
}
