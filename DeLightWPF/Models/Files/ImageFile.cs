using CommunityToolkit.Mvvm.ComponentModel;

namespace DeLightWPF.Models.Files
{
    public partial class ImageFile : ScreenFile
    {
        [ObservableProperty]
        private double duration;

        public ImageFile()
        {
            Duration = 5;
        }
    }
}
