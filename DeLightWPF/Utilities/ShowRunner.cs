using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Models;
using System.Collections.Generic;
using System.Linq;

namespace DeLightWPF.Utilities
{
    public partial class ShowRunner : ObservableObject
    {
        public Show Show { get; set; }

        [ObservableProperty]
        private Cue? selectedCue;
        [ObservableProperty]
        private Cue? activeCue;

        public bool ShowComplete = false;

        public VideoWindow VideoWindow { get; set; }

        public List<CueRunner> ActiveCues { get; set; } = new();

        public ShowRunner(Show show, VideoWindow videoWindow)
        {
            Show = show;
            VideoWindow = videoWindow;
        }
        public void Play()
        {
            if (ShowComplete && SelectedCue == null)
                return;

            // The show was over, but a new cue is manually selected
            ShowComplete = ShowComplete && SelectedCue == null;

            SelectedCue ??= Show.Cues.FirstOrDefault();

            if (SelectedCue != null)
            {
                ActiveCue = SelectedCue;
                int index = Show.Cues.IndexOf(SelectedCue);

                SelectedCue = index < Show.Cues.Count - 1 ? Show.Cues[index + 1] : null;
                ShowComplete = SelectedCue == null;

                VideoWindow.Show();
                CueRunner cueRunner = new(ActiveCue, VideoWindow);
                ActiveCues.Add(cueRunner);
                cueRunner.Play();
            }
        }

        public void Stop()
        {
            foreach (CueRunner cueRunner in ActiveCues)
                cueRunner.Stop();
            ActiveCues.Clear();
        }

    }
}
