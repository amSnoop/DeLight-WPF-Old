using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Models;
using DeLightWPF.Utilities;
using System;

namespace DeLightWPF.ViewModels
{
    public partial class CueViewModel : ObservableObject
    {
        private CuePlayer _cuePlayer = new();
        [ObservableProperty]
        private Cue? currentCue;

        public void Play()
        {
            if(CurrentCue != null)
                _cuePlayer.Play(CurrentCue);
        }

        public void Pause()
        {
            if(CurrentCue != null)
                _cuePlayer.Pause(CurrentCue);
        }

        public void Scrub(TimeSpan newPosition)
        {
            if(CurrentCue != null)
                _cuePlayer.Scrub(CurrentCue, newPosition);
        }
    }
}
