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
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace DeLightNew {
    public partial class VideoWindow : Window {

        public Media Media
        {
            get ;
            set;
        }
        public VideoWindow(LibVLC libVLC) {
            InitializeComponent();
            VideoViewControl.MediaPlayer = new MediaPlayer(libVLC);
            var transparentCursor = new Cursor(new Bitmap("C:\\Untitled.png"), new PixelPoint(0, 0));
            Overlay.Cursor = transparentCursor;
            Overlay.Focusable = true;
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
            FadeInVideo();
            VideoViewControl.MediaPlayer.Volume = volume;
            VideoViewControl.MediaPlayer?.Play(Media);
            VideoViewControl.MediaPlayer?.Play(Media);
        }
        protected override void OnOpened(EventArgs e) {
            base.OnOpened(e);
            Play();
        }
        private void FadeInVideo()
        {
            // Make sure the overlay is initially opaque
            Overlay.Background = new SolidColorBrush(Colors.Black, 1);

            // Create a new animation to fade out the overlay
            var fadeOutAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(2), // adjust as needed
                Easing = new QuadraticEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter
                            {
                                Property = Border.BackgroundProperty,
                                Value = new SolidColorBrush(Colors.Black, 0)
                            }
                        }
                    }
                }
            };

            // Start the animation
            Dispatcher.UIThread.Post(() => fadeOutAnimation.RunAsync(Overlay));
        }
    }
}
