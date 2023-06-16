using DeLightWPF.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace DeLightWPF.Utilities {

    public class CuePlayer {
        public Screen? Screen { get; set; }

        public VideoWindow? VideoWindow { get; set; }
        public VideoWindow? NewVideoWindow { get; set; }

        public async Task Play(Cue? cue) {
            if (VideoWindow == null) {
                VideoWindow = new(cue?.VidPath, Screen) {
                    Opacity = 1
                };
                VideoWindow.Show();
                VideoWindow.Play();
            }
            else {
                if(NewVideoWindow == null || NewVideoWindow.MediaUriString != cue?.VidPath)
                    NewVideoWindow = new(cue?.VidPath, Screen);
                await SwapVideoWindows(cue?.FadeInTime ?? 0);
                VideoWindow?.Close();
                VideoWindow = NewVideoWindow;
                NewVideoWindow = null;
            }
        }
        public void Load(Cue? cue) {
            NewVideoWindow = new(cue?.VidPath, Screen);
            NewVideoWindow.Show();
        }
        public void Pause(Cue cue) {
            // Implement pause logic here
        }
        public async Task Stop(Cue? cue) {
            if (cue == null) {
                VideoWindow?.Close();
                VideoWindow = null;
            }
            else {
                NewVideoWindow = new("", Screen);
                await SwapVideoWindows(cue.FadeOutTime); 
                VideoWindow?.Close();
                VideoWindow = NewVideoWindow;
                NewVideoWindow = null;
            }
        }
        public void HardStop() {
            VideoWindow?.Close();
            VideoWindow = null;
        }
        public void Scrub(Cue cue, TimeSpan newPosition) {
            // Implement scrubbing logic here
        }

        private Task SwapVideoWindows(double dur) {
            NewVideoWindow?.Show();
            NewVideoWindow?.Play();
            var duration = TimeSpan.FromSeconds(dur);
            var fadeIn = new DoubleAnimation(0, 1, duration);
            //var fadeOutAudio = new DoubleAnimation(_window.VolumeSlider.Value, 0.0, duration);

            var tcs = new TaskCompletionSource<bool>();

            fadeIn.Completed += (s, e) => {
                tcs.SetResult(true);
            };

            NewVideoWindow?.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            //_videoWindow?.VideoViewControl.BeginAnimation(MediaElement.VolumeProperty, fadeOutAudio);

            return tcs.Task;
        }
    }
}
