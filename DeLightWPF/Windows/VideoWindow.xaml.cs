using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Controls;
using DeLightWPF.Models;
using System.Windows.Media;
using System.Windows.Forms;

namespace DeLightWPF {
    public partial class VideoWindow : Window {
        public Cue? Cue { get; set; } = new();
        private MediaElement? curVid;
        private MediaElement? newVid;

        public VideoWindow(Screen? screen) {
            InitializeComponent();
            Top = screen?.Bounds.Top ?? Screen.PrimaryScreen?.Bounds.Top ?? 0;
            Left = screen?.Bounds.Left ?? Screen.PrimaryScreen?.Bounds.Left ?? 0;
        }

        public VideoWindow() {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e) {
            Stop();
            base.OnClosed(e);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
            e.Handled = true;
        }
        public void Load(Cue? cue) {
            Cue = cue;
            Load();
        }
        private void Load() {
            if (Cue != null) {
                if (Cue.VidPath != null && Cue.VidPath != "") {
                    newVid = new() {
                        Source = new Uri(Cue.VidPath),
                        LoadedBehavior = MediaState.Manual,
                        UnloadedBehavior = MediaState.Manual,
                        Stretch = Stretch.UniformToFill,
                        Volume = Cue.Volume,
                        Opacity = 0,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    Container.Children.Add(newVid);
                }
            }
        }

        public new void Hide() {
            curVid?.Stop();
            newVid?.Stop();
            curVid = null;
            Load();
            base.Hide();
        }

        public async Task Play() {
            newVid?.Play();
            await Fade();
            curVid?.Stop();
            Container.Children.Remove(curVid);
            curVid = newVid;
            Load();
        }

        public async void Stop() {
            newVid?.Stop();
            Container.Children.Remove(newVid);
            newVid = null;
            await Play();
            Load();
        }

        private async Task Fade() {
            var fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(Cue?.FadeInTime ?? 0));
            var fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(Cue?.FadeOutTime ?? 0));
            var taskCompletionSource = new TaskCompletionSource<bool>();

            fadeInAnimation.Completed += (s, e) => {
                taskCompletionSource.SetResult(true);
            };
            if (newVid != null)
                newVid?.BeginAnimation(OpacityProperty, fadeInAnimation);
            else
                curVid?.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await taskCompletionSource.Task;
        }
    }
}
