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
        WARNING
    }
    public enum EndAction
    {
        Loop,
        FadeAfterEnd,
        FadeBeforeEnd,
        Freeze,
    }
    public enum FadeType
    {
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
        [NotifyPropertyChangedFor(nameof(Type))]
        private string vidPath = "";
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Type))]
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

        public CueType Type
        {
            get
            {
                bool hasVid = IsVideoFileType();
                bool hasImg = IsImageFileType();
                bool hasLight = !string.IsNullOrEmpty(LightPath) && IsLightFileType();

                if (string.IsNullOrEmpty(VidPath) && string.IsNullOrEmpty(LightPath))
                    return CueType.Blackout;
                else if (hasVid && hasLight)
                    return CueType.VidLight;
                else if (hasImg && hasLight)
                    return CueType.ImgLight;
                else if (hasVid)
                    return CueType.VidOnly;
                else if (hasImg)
                    return CueType.ImgOnly;
                else if (hasLight)
                    return CueType.LightOnly;
                else
                    return CueType.WARNING;
            }
        }




        public Cue()
        {
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

        private bool IsImageFileType()
        {
            return VidPath.EndsWith(".jpg") || VidPath.EndsWith(".png") || VidPath.EndsWith(".bmp") || VidPath.EndsWith(".jpeg");
        }
        private bool IsVideoFileType()
        {
            return VidPath.EndsWith(".mp4") || VidPath.EndsWith(".mov") || VidPath.EndsWith(".avi") || VidPath.EndsWith(".wmv");
        }
        private bool IsLightFileType()
        {
            return LightPath.EndsWith(".scex");
        }

    }
}
