using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLightWPF.Models
{
    public class VideoStatus
    {
        public TimeSpan Duration = new();
        public TimeSpan Position = new();
        public bool Error = false;
        public bool HasEnded => Position >= Duration;

        public VideoStatus() { }


    }
}
