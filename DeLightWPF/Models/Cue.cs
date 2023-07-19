using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Utilities;

namespace DeLightWPF.Models
{
    public enum CueType
    {
        Blackout,
        VidOnly,
        ImgOnly,
        VidLight,
        ImgLight,
        LightOnly,
    }
    public partial class Cue : ObservableObject
    {
        [ObservableProperty]
        private string number = "";
        [ObservableProperty]
        private string note = "";
        [ObservableProperty]
        private CueType type;
        [ObservableProperty]
        private string vidPath = "";
        [ObservableProperty]
        private string lightPath = "";

        [ObservableProperty]
        private bool isActive = false;

        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]
        private bool loop;
        [ObservableProperty]
        private double volume;
        [ObservableProperty]
        private double duration;
        public Cue()
        {
            Number = "";
            Note = "";
            Type = CueType.Blackout;
            VidPath = "";
            LightPath = "";
            FadeInTime = GlobalSettings.Instance.DefaultFadeTime;
            FadeOutTime = GlobalSettings.Instance.DefaultFadeTime;
            Loop = false;
            Volume = GlobalSettings.Instance.DefaultVolume;
            Duration = GlobalSettings.Instance.DefaultDuration;
        }
    }
}
