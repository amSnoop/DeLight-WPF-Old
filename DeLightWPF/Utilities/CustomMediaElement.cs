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
        /*
         *  Some important states and what happens in those states
         * 
         *  Cue has just started OR has just looped and video is not set to loop or freeze itself:
         *      Fade in video
         *  Video ends and is set to loop:
         *      Loop video
         *  fadeOutTime reached and video is set to fade out:
         *      Fade out video
         * 
         */



        enum FadeState {
            None,
            FadingIn,
            FadingOut
        }
        public readonly Cue Cue;

        public event EventHandler? FadedIn;

        public event EventHandler? FadedOut;

        private DispatcherTimer timer = new();

        private double fadeTimeRemaining = 0;//updated whenever the slider is moved, and used to determine how much time is left in the next fade animation

        private FadeState fadeState= FadeState.FadingIn; //used to determine if the video should currently be fading in or out once play is pressed

        public bool isFadingOut = false;//used to prevent the fadeout animation from being called multiple times
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
            fadeTimeRemaining = cue.FadeInTime;
        }

        public void Load() {
            Play();
            Stop();
        }

        public void FadeOutEventHandler(object? sender, EventArgs e)
        {
            FadeOut();
        }

        public void PlayFromStart() {
            FadeIn();
        }

        public void TimerTick(object? sender, EventArgs e)
        {
            if( !isFadingOut && Position > NaturalDuration.TimeSpan - TimeSpan.FromSeconds(Cue.FadeOutTime))
            {
                FadeOut();
                isFadingOut = true;
            }
        }

        public void UpdateFadeState()
        {
            //If video should be in the process of fading in(either hte first time the video is playing, or if it's not set to loop)
            if(Position.TotalSeconds < Cue.FadeInTime && (Cue.VidEndAction != EndAction.Loop || LoopCount == 1))
            {
                Opacity = Position.TotalSeconds / Cue.FadeInTime;
                fadeState = FadeState.FadingIn;
                fadeTimeRemaining = Position.TotalSeconds / Cue.FadeInTime;
            }
            //If video should be in the process of fading out
            else if (Cue.VidEndAction == EndAction.FadeBeforeEnd && Position > NaturalDuration.TimeSpan - TimeSpan.FromSeconds(Cue.FadeOutTime))
            {
                Opacity = (NaturalDuration.TimeSpan - Position).TotalSeconds / Cue.FadeOutTime;
                fadeState = FadeState.FadingOut;
            }
            else if (Cue.VidEndAction == EndAction.FadeAfterEnd && Position > NaturalDuration.TimeSpan - TimeSpan.FromSeconds(Cue.FadeOutTime))
            {
                Opacity = (NaturalDuration.TimeSpan - Position).TotalSeconds / Cue.FadeOutTime;
                fadeState = FadeState.FadingOut;
            }
            else
            {
                Opacity = 1;
                fadeState = FadeState.None;
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
                UpdateFadeState();
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
