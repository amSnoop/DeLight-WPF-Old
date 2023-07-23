using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DeLightWPF.Models
{


    /*
     * Structure:
     * VisualCueFile
     *   - ScreenFile
     *      - VideoFile
     *      - GifFile
     *      - ImageFile
     *   - LightFile
     * 
     * Gif and Image file are different because MediaElement is needed for gifs, but not for images, but they can't set a gif duration. Therefore special behaviors are needed.
     * 
     */
    public abstract class CueFile
    {
        public string FilePath { get; set; }
        public EndAction EndAction { get; set; }
        public double FadeInDuration { get; set; }
        public double FadeOutDuration { get; set; }

        public virtual bool HasValidFile => false;

        protected static readonly List<string> videoFileTypes = new (){ ".mp4", ".mov", ".avi", ".wmv" };
        protected static readonly List<string> imageFileTypes = new() { ".jpg", ".jpeg", ".png", ".bmp" };
        protected static readonly List<string> gifFileTypes = new() { ".gif" };
        protected static readonly List<string> lightFileTypes = new() { ".scex" };

        public CueFile()
        {
            FilePath = "";
            EndAction = EndAction.Freeze;
            FadeInDuration = 3;
            FadeOutDuration = 3;
        }
        protected bool IsValidFile(List<string> validExtensions)
        {
            foreach (var extension in validExtensions)
            {
                if (FilePath.EndsWith(extension) && Path.Exists(FilePath))
                    return true;
            }
            return false;
        }
    }

    public class ScreenFile : CueFile
    {
        //Eventually this will probably have information about what screen it should be displayed on, but that's a bit in the future...
    }

    public class VideoFile : ScreenFile
    {
        public double Volume { get; set; }
        public override bool HasValidFile => IsValidFile(videoFileTypes);

        public VideoFile()
        {
            Volume = 1;
        }
    }
    public class GifFile : ScreenFile
    {
        public double Duration { get; set; }
        public override bool HasValidFile => IsValidFile(gifFileTypes);

        public GifFile()
        {
            Duration = 5;
        }
    }
    public class ImageFile : ScreenFile
    {
        public double Duration { get; set; }
        public override bool HasValidFile => IsValidFile(imageFileTypes);

        public ImageFile()
        {
            Duration = 5;
        }
    }
    public class LightFile : CueFile
    {
        public double Duration { get; set; }
        public override bool HasValidFile => IsValidFile(lightFileTypes);
        public LightFile()
        {
            Duration = 0;
        }
    }
}
