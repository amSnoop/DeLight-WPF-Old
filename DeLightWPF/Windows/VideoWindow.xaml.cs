using System;
using System.Windows;
using System.Windows.Input;

namespace DeLightWPF {
    public partial class VideoWindow : Window {
        public Uri? MediaUri { get; set; }

        public VideoWindow(Uri mediaUri)
        {
            InitializeComponent();
            MediaUri = mediaUri;
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
