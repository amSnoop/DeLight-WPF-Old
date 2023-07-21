using DeLightWPF.Models;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DeLightWPF.Utilities
{
    public class CustomMediaElement : MediaElement
    {
        public readonly Cue Cue;

        public event EventHandler? FadedIn;

        public event EventHandler? FadedOut;

        private DispatcherTimer timer = new();

        public bool isFadingOut = false;

        public int LoopCount { get; set; } = 0;
        public CustomMediaElement(Cue cue) : base()
        {
            LoadedBehavior = MediaState.Manual;
            UnloadedBehavior = MediaState.Manual;
            Cue = cue;
            Volume = cue.Volume;
            IsMuted = false;
            Source = new Uri(cue.VidPath);
            Opacity = 0;
            Play();
            Stop();
            FadedOut += OnFadedOut;
            if (cue.VidEndAction == EndAction.Loop)
            {
                MediaEnded += RestartMedia;
            }
            else if (cue.VidEndAction == EndAction.FadeBeforeEnd)
            {
                timer.Tick += TimerTick;
            }
            else if(cue.VidEndAction == EndAction.FadeAfterEnd)
            {
                MediaEnded += FadeOutEventHandler;
            }
        }

        public void FadeOutEventHandler(object? sender, EventArgs e)
        {
            FadeOut();
        }

        public void TimerTick(object? sender, EventArgs e)
        {
            if( !isFadingOut && Position > NaturalDuration.TimeSpan - TimeSpan.FromSeconds(Cue.FadeOutTime))
            {
                FadeOut();
                isFadingOut = true;
            }
        }

        public void TestFadeTiming()
        {
            //If video should be in the process of fading in(either hte first time the video is playing, or if it's not set to loop)
            if(Position.TotalSeconds < Cue.FadeInTime && (Cue.VidEndAction != EndAction.Loop || LoopCount == 1))
            {
                Opacity = Position.TotalSeconds / Cue.FadeInTime;
                FadeIn(Cue.FadeInTime - Position.TotalSeconds);
            }
            //If video should be in the process of fading out
            else if (Cue.VidEndAction == EndAction.FadeBeforeEnd && Position > NaturalDuration.TimeSpan - TimeSpan.FromSeconds(Cue.FadeOutTime))
            {
                Opacity = (NaturalDuration.TimeSpan - Position).TotalSeconds / Cue.FadeOutTime;
                FadeOut(Cue.FadeOutTime - (NaturalDuration.TimeSpan - Position).TotalSeconds);
            }
            else if(Cue.VidEndAction == EndAction.FadeAfterEnd && Position > NaturalDuration.TimeSpan - TimeSpan.FromSeconds(Cue.FadeOutTime))
            {
                Opacity = (NaturalDuration.TimeSpan - Position).TotalSeconds / Cue.FadeOutTime;
                FadeOut()
            }
            else
            {
                Opacity = 0;
            }
        }

        private void RestartMedia(object? sender, RoutedEventArgs e)
        {
            Stop();
            Position = TimeSpan.Zero;
            Play();
            LoopCount++;
        }

        public void SetTickPosition(int tick)
        {
            LoopCount = 0;
            //tick is 1/20th of a second, set that to the media's position, iterating hte loop count if the media is shorter than the requested position, and playing from that spot
            var timeStamp = TimeSpan.FromSeconds(tick * 20.0);
            if (NaturalDuration.HasTimeSpan)
            {
                if (Cue.VidEndAction == EndAction.Loop)
                {
                    while (timeStamp > NaturalDuration.TimeSpan)
                    {
                        LoopCount++;
                        timeStamp -= NaturalDuration.TimeSpan;
                    }
                    Position = timeStamp;
                }
                else
                {
                    Position = timeStamp;
                }
                TestFadeTiming();
            }
        }

        public void OnFadedOut(object? s, EventArgs e)
        {
            Stop();
            timer.Stop();
            isFadingOut = false;
        }


        public void FadeIn(double duration = -1)
        {
            Position = TimeSpan.Zero;
            Play();
            LoopCount++;
            DoubleAnimation fadeIn = new(1, TimeSpan.FromSeconds(duration == -1 ? Cue.FadeInTime : duration));
            fadeIn.Completed += (s, e) => FadedIn?.Invoke(this, EventArgs.Empty);
            BeginAnimation(fadeIn);
        }

        public void FadeOut(double duration = -1)
        {
            DoubleAnimation fadeOut = new(0, TimeSpan.FromSeconds(duration == -1 ? Cue.FadeOutTime : duration));
            fadeOut.Completed += (s, e) => FadedOut?.Invoke(this, EventArgs.Empty);
            BeginAnimation(fadeOut);
        }

        public void BeginAnimation(DoubleAnimation animation)
        {
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }


}
