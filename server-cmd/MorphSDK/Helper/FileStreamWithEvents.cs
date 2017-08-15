using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Helper
{
    internal class FileStreamWithEvents : FileStream
    {
        public event EventHandler<long> PositionChanged;
        public FileStreamWithEvents(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {

        }
        public override int Read(byte[] array, int offset, int count)
        {
            if (PositionChanged != null)
            {
                PositionChanged(this, this.Position);
            }
            return base.Read(array, offset, count);
        }
    }
}
