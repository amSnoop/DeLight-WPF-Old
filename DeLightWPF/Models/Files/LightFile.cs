using CommunityToolkit.Mvvm.ComponentModel;

namespace DeLightWPF.Models.Files
{
    public partial class LightFile : CueFile, ILightFile
    {
        [ObservableProperty]
        private double duration;
        public LightFile()
        {
            Duration = 0;
        }
    }
}
