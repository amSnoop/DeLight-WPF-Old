using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using LibVLCSharp.Shared;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using Avalonia.Platform;
using Avalonia.Media.Imaging;

namespace DeLightNew {
    public partial class VideoWindow : Window {

        private int count = 0;

        public Media Media
        {
            get ;
            set;
        }
        public VideoWindow(LibVLC libVLC) {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            VideoViewControl.MediaPlayer = new MediaPlayer(libVLC);
            //Overlay.PointerEnter += OnMove;
            //Overlay.Focusable = true;
            Focusable = false;
        }
        public VideoWindow() {
            InitializeComponent();
        }
        protected override void OnClosed(EventArgs e)
        {
            VideoViewControl.MediaPlayer?.Pause();
            VideoViewControl.MediaPlayer?.Stop();
            base.OnClosed(e);
        }

        public override void Hide()
        {
            VideoViewControl.MediaPlayer?.Pause();
            VideoViewControl.MediaPlayer?.Stop();
            base.Hide();
        }
        public void Play(int volume = 20)
        {
            VideoViewControl.MediaPlayer.Volume = volume;
            VideoViewControl.MediaPlayer?.Play(Media);
            VideoViewControl.MediaPlayer?.Play(Media);
        }
        protected override void OnOpened(EventArgs e) {
            base.OnOpened(e);
            Play();
        }
        protected void OnMove(object? sender, PointerEventArgs e)
        {
            var transparentCursor = new Cursor(new Bitmap("transparent.png"), new PixelPoint(0, 0));
            Overlay.Cursor = new Cursor(StandardCursorType.None);
            //VideoViewControl.Cursor = transparentCursor;
            //Cursor = transparentCursor;
            e.Handled = true;
        }
    }
}
