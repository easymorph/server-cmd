using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Events
{
    public class FileEventArgs : EventArgs
    {
        public FileProgressState State { get; set; }
        public long ProcessedBytes { get; set; }
        public long FileSize { get; set; }
        public Guid? Guid { get; set; }
        public string FileName { get; set; }
        public double Percent
        {
            get
            {
                if (FileSize == 0)
                    return 0;
                return Math.Round((ProcessedBytes * 100.0 / FileSize), 2);
            }
        }
        public FileEventArgs()
        {


        }

    }
}
