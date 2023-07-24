using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLightWPF.Models.Files
{
    public static class CueFactory
    {
        private static readonly Dictionary<string, Func<string, CueFile>> cueCreators = new()
    {
        { ".mp4", path => new VideoFile { FilePath = path } },
        { ".mov", path => new VideoFile { FilePath = path } },
        { ".avi", path => new VideoFile { FilePath = path } },
        { ".wmv", path => new VideoFile { FilePath = path } },
        { ".jpg", path => new ImageFile { FilePath = path } },
        { ".jpeg", path => new ImageFile { FilePath = path } },
        { ".png", path => new ImageFile { FilePath = path } },
        { ".bmp", path => new ImageFile { FilePath = path } },
        { ".gif", path => new GifFile { FilePath = path } },
        { ".scex", path => new LightFile { FilePath = path } },
        { ".mp3", path => new AudioFile { FilePath = path } },
        { ".wav", path => new AudioFile { FilePath = path } },
        { ".m4a", path => new AudioFile { FilePath = path } },
    };

        public static CueFile CreateCueFile(string filePath, CueFile? oldCue = null)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            if (oldCue != null)
            {
                if (Path.GetExtension(oldCue.FilePath).ToLower() == extension)
                {
                    oldCue.FilePath = filePath;
                    return oldCue;
                }
            }
            if (cueCreators.TryGetValue(extension, out var creator))
            {
                CueFile c = creator(filePath);
                if (oldCue != null)
                {
                    c.EndAction = oldCue.EndAction;
                    c.FadeInDuration = oldCue.FadeInDuration;
                    c.FadeOutDuration = oldCue.FadeOutDuration;
                }
                return c;
            }
            else
            {
                throw new Exception("File extension not supported.");
            }
        }
    }

}
