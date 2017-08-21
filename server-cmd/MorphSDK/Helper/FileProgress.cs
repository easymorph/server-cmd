using MorphSDK.Events;
using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Helper
{

    public class FileProgress : IFileProgress
    {
        public event EventHandler<FileEventArgs> StateChanged;

        public long FileSize { get; private set; }
        public string FileName { get; private set; }
        private Guid? _guid;
        public long ProcessedBytes { get; private set; }
        public FileProgressState State { get; private set; }

        public void ChangeState(FileProgressState state)
        {
            State = state;
            if (StateChanged != null)
            {
                StateChanged(this, new FileEventArgs
                {
                    ProcessedBytes = ProcessedBytes,
                    State = state,
                    Guid = _guid,
                    FileName = FileName,
                    FileSize = FileSize

                });
            }
        }
        public void SetProcessedBytes(long np)
        {
            ProcessedBytes = np;
        }

        public FileProgress(string fileName, long fileSize, Guid? guid = null)
        {
            FileName = fileName;
            FileSize = fileSize;
            _guid = guid;
        }
    }
}
