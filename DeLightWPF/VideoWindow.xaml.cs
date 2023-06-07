using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DeLightWPF {
    public partial class VideoWindow : Window {
        public Uri? MediaUri { get; set; }

        public VideoWindow(Uri mediaUri) {
            InitializeComponent();
            MediaUri = mediaUri;
            Focusable = false;
            Cursor = Cursors.None;
        }

        public VideoWindow() {
            InitializeComponent();
            Focusable = false;
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

        public void Play(double volume = 0.2) {
            VideoViewControl.Volume = volume;
            VideoViewControl.Source = MediaUri;
            VideoViewControl.Play();
        }

        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            Play();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = true;
        }
    }
}
