using CommunityToolkit.Mvvm.ComponentModel;

namespace DeLightWPF.Models.Files
{
    public partial class GifFile : ScreenFile
    {
        [ObservableProperty]
        private double duration;

        public GifFile()
        {
            Duration = 5;
        }
    }
}
