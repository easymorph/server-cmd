using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Helper
{
    internal class ProgressStreamContent : HttpContent
    {
        private const int DefBufferSize = 4096;

        private Stream _stream;
        private int _bufSize;
        private bool _consumed;
        private IFileProgress _fileProgress;
        private DateTime _lastUpdate = DateTime.MinValue;



        public ProgressStreamContent(Stream stream, int bufSize, IFileProgress fileProgress)
        {
            _stream = stream;
            _bufSize = bufSize;
            _fileProgress = fileProgress;
        }

        public ProgressStreamContent(Stream stream, IFileProgress downloader) : this(stream, DefBufferSize, downloader) { }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[_bufSize];
            var size = _stream.Length;
            var processed = 0;

            _fileProgress.ChangeState(FileProgressState.Starting);

            using (_stream)
            {
                while (true)
                {
                    var length = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (length <= 0) break;
                                        
                    await stream.WriteAsync(buffer, 0, length);
                    processed += length;                    

                    if (DateTime.Now - _lastUpdate > TimeSpan.FromMilliseconds(250))
                    {
                        _fileProgress.SetProcessedBytes(processed);
                        _fileProgress.ChangeState(FileProgressState.Processing);
                        _lastUpdate = DateTime.Now;
                    }
                }
            }

            _fileProgress.ChangeState(FileProgressState.Finishing);

        }

        protected override bool TryComputeLength(out long length)
        {
            length = _stream.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
            }
            base.Dispose(disposing);
        }

      
    }
}
