﻿using DeLightWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace DeLightWPF.Utilities
{

    public class Light : IRunnableVisualCue
    {
        public double Duration { get; set; } = Math.Round(new Random().NextDouble() * 10, 1);
        public CueFile File { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double Opacity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsFadingOut => throw new NotImplementedException();

        double? IRunnableVisualCue.Duration => throw new NotImplementedException();

        public event EventHandler? FadedIn;
        public event EventHandler? FadedOut;
        public event EventHandler? PlaybackEnded;

        public void ClearCurrentAnimations()
        {
            throw new NotImplementedException();
        }

        public void FadeIn(double duration = -1)
        {
            throw new NotImplementedException();
        }

        public void FadeOut(double duration = -1)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void SeekTo(double time)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }
    }

    public class RequestFadeOutEventArgs : EventArgs
    {
        public double Duration { get; set; }
    }

    public class CueRunner
    {
        private readonly double OPACITY_FULL = 1;//chatGPT doesn't like magic numbers, and GPT is my code reviewer, so here we are
        private readonly double OPACITY_NONE = 0;


        public event EventHandler<RequestFadeOutEventArgs>? RequestedFadeToBlack;
        public Cue Cue { get; set; }

        //insert list of IRunnableVisualCues here
        public List<IRunnableVisualCue> VisualCues { get; set; } = new();
        public DispatcherTimer Timer { get; set; }
        public int LoopCount { get; set; } = 0;
        public int ElapsedTicks { get; set; } = 0;

        public Light Light = new();

        public double RealDuration { get; set; } = 0;


        public CueRunner(Cue cue)
        {
            Cue = cue;
            Timer = new()
            {
                Interval = TimeSpan.FromMilliseconds(GlobalSettings.TickRate)
            };
            Timer.Tick += Timer_Tick;
            Light l = new();
            VisualCues.Add(l);
            DetermineFileEndingEvent(l);
            foreach (var sf in cue.ScreenFiles)
            {
                //TODO: Add support for other types of cues
                CustomMediaElement cme;
                if (sf is VideoFile)
                    cme = new(sf);
                else if (sf is GifFile)
                    cme = new(sf);
                else if (sf is ImageFile)
                    cme = new(sf);
                else
                    throw new Exception("Unknown VisualCue type, dumbass. wtf u doin boi");
                DetermineFileEndingEvent(cme);
            }
        }

        private void DetermineFileEndingEvent(IRunnableVisualCue vc)
        {
            if (vc.File.EndAction == EndAction.FadeAfterEnd)
                vc.PlaybackEnded += (s, e) => vc.FadeOut();
            else if (vc.File.EndAction == EndAction.Loop)
                vc.PlaybackEnded += (s, e) => vc.Restart();
            else if (vc.File.EndAction == EndAction.FadeBeforeEnd)
                Timer.Tick += FileEndWatch;

        }
        private void FileEndWatch(object? sender, EventArgs e)
        {
            foreach (var vc in VisualCues)
                if (!vc.IsFadingOut && vc.Duration - (ElapsedTicks * GlobalSettings.TickRate) <= vc.File.FadeOutDuration)
                    vc.FadeOut();
        }

        public void FindRealCueDuration()
        {
            if (Cue.Duration == 0)
                foreach (var vc in VisualCues)
                    if ((vc.Duration ?? 0) > RealDuration)
                        RealDuration = vc.Duration ?? 0;
                    else
                        RealDuration = Cue.Duration;
        }

        public void Timer_Tick(object? s, EventArgs e)
        {
            ElapsedTicks++;
            if (ElapsedTicks >= RealDuration * 1000 / GlobalSettings.TickRate)
            {
                if (Cue.CueEndAction == EndAction.Loop)
                {
                    LoopCount++;
                    Play();
                }
                else if (Cue.CueEndAction == EndAction.FadeAfterEnd)
                    End();
            }
            else if (ElapsedTicks * GlobalSettings.TickRate >= RealDuration - Cue.FadeOutTime && Cue.CueEndAction == EndAction.FadeBeforeEnd)
                End();
        }

        //Plays from the beginning of the cue
        public async void Play()
        {

            //wait for media to open before attempting to find its duration
            var loadTasks = VisualCues.Select(vc => vc.LoadAsync()).ToList();
            await Task.WhenAll(loadTasks);


            //if HasTimeSpan returns false, then some real shenanigans are afoot... Should probably throw an exception or something but whatever (TODO) :P
            //That comment brought to you by GitHub Copilot - Surprisingly funny lmao
            FindRealCueDuration();
            SeekTo(0, true);
            ElapsedTicks = 0;
            Timer.Start();
        }
        public void End()
        {
            ElapsedTicks = 0;
            Timer.Stop();
            foreach (var vc in VisualCues)
            {
                vc.FadeOut(Cue.FadeOutTime);

            }

        }
        public void Pause()
        {
            Timer.Stop();
            SeekTo(ElapsedTicks, false);
        }
        public void Unpause()
        {
            Timer.Start();
            SeekTo(ElapsedTicks, true);
        }
        public void Stop()
        {
            Timer.Stop();
            ElapsedTicks = 0;
            foreach (var vc in VisualCues)
                vc.Stop();
        }

        public void SeekTo(int tick, bool play = false)
        {
            ElapsedTicks = tick;
            double seconds = tick * GlobalSettings.TickRate / 1000.0;
            foreach (var vc in VisualCues)
            {
                Pause();
                vc.ClearCurrentAnimations();
                if (seconds < Cue.FadeInTime)
                    SeekedToFadeIn(vc, seconds, play);
                else if (seconds > vc.Duration)
                    SeekedToAfterEnd(vc, seconds, play);
                else if (vc.File.EndAction == EndAction.FadeBeforeEnd && (vc.Duration - seconds) < Cue.FadeOutTime)
                    SeekedToFadeBeforeEnd(vc, seconds, play);
                else
                    SeekedToNormalPlayback(vc, seconds, play);
            }

        }

        private void SeekedToFadeIn(IRunnableVisualCue vc, double time, bool play)
        {
            vc.Opacity = time / Cue.FadeInTime;
            vc.SeekTo(time);
            if (play)
                vc.FadeIn(Cue.FadeInTime - time);
        }

        private void SeekedToAfterEnd(IRunnableVisualCue vc, double time, bool play)
        {
            time = Math.Max(time, 0);
            if (vc.File.EndAction == EndAction.Freeze)
            {
                vc.Opacity = OPACITY_FULL;
                vc.SeekTo(time);
            }
            else if (vc.File.EndAction == EndAction.FadeBeforeEnd)
            {
                vc.Opacity = OPACITY_NONE;
                vc.SeekTo(time);
            }
            else if (vc.File.EndAction == EndAction.FadeAfterEnd)
            {
                vc.Opacity = (time - vc.Duration ?? 0) / Cue.FadeOutTime;
                vc.SeekTo(time);
                if (play)
                    vc.FadeOut(Cue.FadeOutTime - (time - vc.Duration ?? 0));
            }
            else if (vc.File.EndAction == EndAction.Loop)
            {
                vc.Opacity = OPACITY_FULL;
                while (time > vc.Duration)
                    time -= vc.Duration ?? 1;
                vc.SeekTo(time);
                if (play)
                    vc.Play();
            }
        }
        private void SeekedToFadeBeforeEnd(IRunnableVisualCue vc, double time, bool play)
        {
            vc.Opacity = (time - vc.Duration ?? 0) / Cue.FadeOutTime;
            vc.SeekTo(time);
            if (play)
                vc.FadeOut(Cue.FadeOutTime - (vc.Duration ?? 0 - time));
        }
        private void SeekedToNormalPlayback(IRunnableVisualCue vc, double time, bool play)
        {
            vc.Opacity = OPACITY_FULL;
            vc.SeekTo(time);
            if (play)
                vc.Play();
        }
    }
}
