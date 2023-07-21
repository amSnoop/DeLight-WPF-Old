using DeLightWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeLightWPF.Utilities {

    public class Light {
        public double Duration { get; set; } = Math.Round(new Random().NextDouble() * 10, 1);
    }
    public class CueRunner {

        public Cue Cue { get; set; }
        public CustomMediaElement MediaElement { get; set; }
        public DispatcherTimer Timer { get; set; }
        public int LoopCount { get; set; } = 0;
        public int ElapsedTicks { get; set; } = 0;

        public Light Light = new();

        public int RealDuration { get; set; } = 0;

        private TaskCompletionSource<bool> tcs = new();


        public CueRunner(Cue cue) {
            Cue = cue;
            Timer = new();
            MediaElement = new(cue);
            MediaElement.MediaOpened += OnMediaOpened;
            MediaElement.Load();//opens media so that its duration can be found
            Timer.Interval = TimeSpan.FromMilliseconds(50);
            Timer.Tick += Timer_Tick;
        }
        public double FindRealCueDuration(double vidEnd, double lightEnd) {

            if (Cue.Duration == 0) {
                if (vidEnd > lightEnd)
                    return vidEnd;
                else
                    return lightEnd;
            }
            else
                return Cue.Duration;
        }

        public void Timer_Tick(object? s, EventArgs e) {
              ElapsedTicks++;
            if (ElapsedTicks >= RealDuration * 20) {
                Timer.Stop();
            }
        }

        private void OnMediaOpened(object? s, EventArgs e) {
            tcs.SetResult(true);
        }

        //Plays from the beginning of the cue
        public async void Play() {
            //wait for media to open before attempting to find its duration
            await tcs.Task;
            //if HasTimeSpan returns false, then some real shenanigans are afoot... Should probably throw an exception or something but whatever (TODO) :P
            //That comment brought to you by GitHub Copilot - Surprisingly funny lmao
            FindRealCueDuration(MediaElement.NaturalDuration.HasTimeSpan ? MediaElement.NaturalDuration.TimeSpan.TotalSeconds : 0, Light.Duration);
            MediaElement.FadeIn();
        }
    }
}
