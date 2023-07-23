using DeLightWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLightWPF.Utilities
{
    public class ShowRunner
    {
        public Show Show { get; set; }

        public Queue<CueRunner> ActiveCues { get; set; } = new();

        public Cue? SelectedCue { get; set; }

        public Cue? ActiveCue { get; set; }

        public VideoWindow VideoWindow { get; set; }


        public void Play()
        {
            SelectedCue ??= Show.Cues.First();
            ActiveCues.Enqueue(new CueRunner(SelectedCue));
            ActiveCues.Peek().Play();
        }



    }
}
