using CommunityToolkit.Mvvm.ComponentModel;

namespace DeLightWPF.Models.Files
{
    public partial class VideoFile : ScreenFile
    {
        [ObservableProperty]
        private double volume;

        public VideoFile()
        {
            Volume = 1;
        }
    }
}
