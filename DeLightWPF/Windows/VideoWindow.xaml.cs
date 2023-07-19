using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using DeLightWPF.Models;
using System.IO;
using System.Windows.Forms;

namespace DeLightWPF
{
    public partial class VideoWindow : Window
    {

        //Queue of currently transitioning media elements. During normal operations, will only have a single element, the one being played.
        //During transitions, will have multiple elements.
        private readonly Queue<MediaElement> _mediaElements = new();
        private MediaElement? _currentMediaElement;

        public VideoWindow(Screen? screen)
        {
            InitializeComponent();
            Top = screen?.Bounds.Top ?? Screen.PrimaryScreen?.Bounds.Top ?? 0;
            Left = screen?.Bounds.Left ?? Screen.PrimaryScreen?.Bounds.Left ?? 0;
        }

        public void SetScreen(Screen screen)
        {
            WindowState = WindowState.Normal;
            Top = screen.Bounds.Top;
            Left = screen.Bounds.Left;
            WindowState = WindowState.Maximized;
        }

        public VideoWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            Stop();
            ClearAllMediaElements();
            base.OnClosed(e);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
        public void SetMasterVol(double value)
        {
            foreach (MediaElement mediaElement in Container.Children)
            {
                mediaElement.Volume ;//do this i nthe other class so you can use the cue volume.
            }
        }
        //returns false if a video file cannot be played or is not found, but there is a string provided in the path.
        public bool Play(Cue? cue)
        {

            bool foundVideo = true;

            if (_mediaElements.Count > 0)//if current transition is happening
                CompleteTransition();

            if (cue != null)
            {
                TimeSpan fadeInDuration = TimeSpan.FromSeconds(cue.FadeInTime);
                cue.VidPath = cue.VidPath.Replace("file:///", "").Trim('\"');

                if (!string.IsNullOrEmpty(cue.VidPath))
                {
                    if (Path.Exists(cue.VidPath))
                        foundVideo = SetupNewMediaElement(cue, fadeInDuration);
                    else
                    {
                        FadeOutCurrentMediaElement(fadeInDuration);
                        foundVideo = false;
                    }
                }
            }
            else
                FadeOutCurrentMediaElement(TimeSpan.FromSeconds(3));
            return foundVideo;
        }
        private bool SetupNewMediaElement(Cue cue, TimeSpan fadeInDuration)
        {
            try
            {
                _currentMediaElement = new MediaElement
                {
                    Source = new Uri(cue.VidPath),
                    Volume = cue.Volume,
                    IsMuted = false,
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Manual,
                    Opacity = 0
                };

                Container.Children.Add(_currentMediaElement);
                _currentMediaElement.Play();
                _mediaElements.Enqueue(_currentMediaElement);

                FadeIn(_currentMediaElement, fadeInDuration, RemoveOldMediaElementIfPresent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing video: [" + cue.VidPath + "]");
                Console.WriteLine(ex.Message);
                FadeOutCurrentMediaElement(fadeInDuration);
                return false;
            }
        }

        private void FadeOutCurrentMediaElement(TimeSpan duration)
        {
            FadeOut(_currentMediaElement, duration, RemoveOldMediaElementIfPresent);
        }

        private void RemoveOldMediaElementIfPresent()
        {
            if (_mediaElements.Count <= 1) return;

            MediaElement oldMediaElement = _mediaElements.Dequeue();
            oldMediaElement.Stop();
            Container.Children.Remove(oldMediaElement);
        }

        public void Stop()
        {
            _currentMediaElement?.Stop();
            ClearAllMediaElements();
        }

        public new void Hide()
        {
            this.Visibility = Visibility.Hidden;
            ClearAllMediaElements();
        }

        private void CompleteTransition()
        {
            while (_mediaElements.Count > 1)
            {
                MediaElement mediaElement = _mediaElements.Dequeue();
                mediaElement.Stop();
                Container.Children.Remove(mediaElement);
            }

            _mediaElements.Peek().Opacity = 1;
        }

        private void ClearAllMediaElements()
        {
            while (_mediaElements.Count > 0)
            {
                MediaElement mediaElement = _mediaElements.Dequeue();
                mediaElement.Stop();
                Container.Children.Remove(mediaElement);
            }
            _currentMediaElement = null;
        }

        private void FadeIn(UIElement element, TimeSpan duration, Action onCompleted)
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation(1, duration);
            fadeInAnimation.Completed += (s, e) => onCompleted?.Invoke();
            StartAnimation(element, fadeInAnimation);
        }

        private void FadeOut(UIElement? element, TimeSpan duration, Action onCompleted)
        {
            if (element == null) return;
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(0, duration);
            fadeOutAnimation.Completed += (s, e) => onCompleted?.Invoke();
            StartAnimation(element, fadeOutAnimation);
        }

        private static void StartAnimation(UIElement element, DoubleAnimation animation)
        {
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
