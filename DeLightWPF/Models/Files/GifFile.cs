using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Interfaces;

namespace DeLightWPF.Models.Files
{
    public partial class GifFile : ScreenFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;

        public GifFile()
        {
            Duration = 5;
        }
    }
}
