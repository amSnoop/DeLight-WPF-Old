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
    public enum EndAction {
        Loop,
        FadeAfterEnd,
        FadeBeforeEnd,
        Freeze,
    }
    public enum FadeType {
        ShowXPress,
        FadeOver,
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
        private double volume;//0 to 1
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private EndAction lightEndAction;
        [ObservableProperty]
        private EndAction vidEndAction;
        [ObservableProperty]
        private FadeType fadeType;

        public Cue() {
            Number = "0";
            FadeInTime = 3;
            FadeOutTime = 3;
            Volume = .2;
            Note = "New Cue";
            Duration = 0;
            Loop = false;
            VidEndAction = EndAction.Freeze;
            LightEndAction = EndAction.Freeze;
            FadeType = FadeType.FadeOver;
        }
    }
}
