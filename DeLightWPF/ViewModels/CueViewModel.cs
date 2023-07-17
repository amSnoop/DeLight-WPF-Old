using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Models;
using DeLightWPF.Utilities;
using System;

namespace DeLightWPF.ViewModels
{
    public partial class CueViewModel : ObservableObject
    {
        [ObservableProperty]
        private Cue? currentCue;

        public string Title => CurrentCue == null ? "No Cue Selected" : "Settings for Cue #" + CurrentCue.Number;

        public string Note => CurrentCue?.Note ?? "";

        public string FormattedNumber => CurrentCue == null ? "" : "#" + CurrentCue.Number + ": ";

        #region Verified Properties
        public double FadeInTime {
            get => CurrentCue?.FadeInTime ?? GlobalSettings.Instance.DefaultFadeTime;
            set
            {
                if(value < 0)
                    value = 0;
                if (CurrentCue != null) {
                    CurrentCue.FadeInTime = value;
                    OnPropertyChanged(nameof(FadeInTime));
                }
            }
        }
        public double FadeOutTime {
            get => CurrentCue?.FadeOutTime ?? GlobalSettings.Instance.DefaultFadeTime;
            set
            {
                if (value < 0)
                    value = 0;
                if (CurrentCue != null) {
                    CurrentCue.FadeOutTime = value;
                    OnPropertyChanged(nameof(FadeOutTime));
                }
            }
        }

        public double Volume {
              get => CurrentCue?.Volume ?? GlobalSettings.Instance.DefaultVolume;
            set
            {
                if (value < 0)
                    value = 0;
                if(value > 1)
                    value = 1;
                if (CurrentCue != null) {
                    CurrentCue.Volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public double Duration
        {
            get => CurrentCue?.Duration ?? GlobalSettings.Instance.DefaultDuration;
            set
            {
                if (value < 0)
                    value = 0;
                if (CurrentCue != null)
                {
                    CurrentCue.Duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        #endregion

        partial void OnCurrentCueChanged(Cue? value)
        {
            OnPropertyChanged(nameof(FadeInTime));
            OnPropertyChanged(nameof(FadeOutTime));
            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Note));
            OnPropertyChanged(nameof(FormattedNumber));
        }
    }
}
