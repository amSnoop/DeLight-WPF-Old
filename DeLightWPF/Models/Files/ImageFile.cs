using CommunityToolkit.Mvvm.ComponentModel;
using DeLightWPF.Interfaces;

namespace DeLightWPF.Models.Files
{
    public partial class ImageFile : ScreenFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;

        public ImageFile()
        {
            Duration = 5;
        }
    }
}
