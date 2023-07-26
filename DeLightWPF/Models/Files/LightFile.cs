using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Interfaces;

namespace DeLightWPF.Models.Files
{
    public partial class LightFile : CueFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;
        public LightFile()
        {
            Duration = 0;
        }
    }
}
