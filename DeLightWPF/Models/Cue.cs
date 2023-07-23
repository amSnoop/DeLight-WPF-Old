using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Utilities;
using System.Collections.Generic;
using System.Windows.Documents;

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
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]
        private double volume;//0 to 1
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private FadeType fadeType;
        [ObservableProperty]
        private EndAction cueEndAction;
        [ObservableProperty]
        private List<ScreenFile> screenFiles;
        [ObservableProperty]
        private LightFile lightScene = new();


        public Cue()
        {
            Number = "0";
            FadeInTime = 3;
            FadeOutTime = 3;
            Volume = .2;
            Note = "New Cue";
            Duration = 0;
            CueEndAction = EndAction.FadeAfterEnd;
            FadeType = FadeType.FadeOver;
            screenFiles = new()
            {
                new VideoFile()
            };
        }

    }
}
