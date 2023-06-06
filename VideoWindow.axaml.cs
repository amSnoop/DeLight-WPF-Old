using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using LibVLCSharp.Shared;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using Avalonia.Platform;

namespace DeLightNew {
    public partial class VideoWindow : Window {

        private int count = 0;
        private Media _media;
        public VideoWindow(LibVLC libVLC, Media m) {
            _media = m;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            VideoViewControl.MediaPlayer = new MediaPlayer(libVLC);
            Topmost = true;
            VideoViewControl.PointerMoved += OnMove;
            Cursor = new Cursor(StandardCursorType.None);
        }
        public VideoWindow() {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) {
                count++;
                if (count == 2) {
                    VideoViewControl.MediaPlayer?.Pause();
                    VideoViewControl.MediaPlayer?.Stop();
                    this.Close();
                    count = 0;
                }
            }
            else
                count = 0;
        }
        protected override void OnOpened(EventArgs e) {
            base.OnOpened(e);
            VideoViewControl.MediaPlayer?.Play(_media);
        }
        protected void OnMove(object? sender, PointerEventArgs e) {
            Cursor = new Cursor(StandardCursorType.None);
            e.Handled = true;
        }
    }
}
