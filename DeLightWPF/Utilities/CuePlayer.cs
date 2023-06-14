using DeLightWPF.Models;
using System;
using System.Windows.Forms;

namespace DeLightWPF.Utilities
{

    public class CuePlayer
    {
        Screen? Screen { get; set; }

        public void Play(Cue cue)
        {
            // Implement play logic here
        }

        public void Pause(Cue cue)
        {
            // Implement pause logic here
        }

        public void Scrub(Cue cue, TimeSpan newPosition)
        {
            // Implement scrubbing logic here
        }
    }
}
