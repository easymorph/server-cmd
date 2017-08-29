using MorphSDK.Events;
using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Helper
{
    public interface IFileProgress
    {
        event EventHandler<FileEventArgs> StateChanged;
        FileProgressState State { get; }
        long FileSize { get; }
        string FileName { get; }
        void SetProcessedBytes(long np);
        void ChangeState(FileProgressState state);



    }
}
