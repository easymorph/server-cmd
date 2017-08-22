using MorphSDK.Events;
using MorphSDK.Model;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace MorphSDK.Client
{
    public interface IMorphServerApiClient
    {
        Task DownloadFileAsync(string spaceName, string path, Func<DownloadFileInfo, bool> handleFile, Stream streamToWriteTo, CancellationToken cancellationToken);
        Task<RunningTaskStatus> GetRunningTaskStatusAsync(string spaceName, Guid taskId, CancellationToken cancellationToken);
        Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken);
        Task UpdateFileAsync(string spaceName, string localFilePath, string destServerFolder, CancellationToken cancellationToken);
        Task StartTaskAsync(string spaceName, Guid taskId, CancellationToken cancellationToken);
        Task StopTaskAsync(string spaceName, Guid taskId, CancellationToken cancellationToken);
        Task UploadFile(string spaceName, string localFilePath, string destServerFolder, CancellationToken cancellationToken);
        Task<bool> IsFileExists(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken);
        Task<SpaceBrowsingInfo> BrowseSpace(string spaceName, string folder, CancellationToken cancellationToken);
        event EventHandler<FileEventArgs> FileProgress;
    }
}