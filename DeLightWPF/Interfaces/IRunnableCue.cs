﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeLightWPF.Models
{
    public interface IRunnableVisualCue 
    {
        CueFile File { get; set; }
        double Opacity { get; set; }

        bool IsFadingOut { get; }
        event EventHandler? FadedIn, FadedOut, PlaybackEnded;

        public double? Duration { get; }

        public void Play();
        public void Pause();
        public void Stop();
        public void SeekTo(double time);
        public void FadeIn(double duration = -1);
        public void FadeOut(double duration = -1);
        public void Restart();
        public void ClearCurrentAnimations();

        public Task LoadAsync();

    }
}
