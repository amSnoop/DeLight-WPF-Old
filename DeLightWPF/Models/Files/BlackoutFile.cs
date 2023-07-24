using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLightWPF.Models.Files
{
    //used if the filepath is not valid
    public partial class BlackoutFile : CueFile, IVisualFile, ILightFile
    {
        public string Reason { get; set; }
        public BlackoutFile(string reason)
        {
            Reason = reason;
        }
    }
}
