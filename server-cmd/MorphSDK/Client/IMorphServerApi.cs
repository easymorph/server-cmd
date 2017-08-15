using MorphSDK.Model;
using System;
using System.IO;
using System.Threading.Tasks;


namespace MorphSDK.Client
{
    public interface IMorphServerApiClient
    {
        Task<DownloadFileInfo> DownloadFileAsync(string spaceName, string serverFilePath, Stream streamToWriteTo);
        Task<RunningTaskStatus> GetRunningTaskStatusAsync(string spaceName, Guid taskId);
        Task<ServerStatus> GetServerStatusAsync();
        Task UpdateFileAsync(string spaceName, string localFilePath, string destServerFolder);
        Task StartTaskAsync(string spaceName, Guid taskId);
        Task StopTaskAsync(string spaceName, Guid taskId);
        Task UploadNewFileAsync(string spaceName, string localFilePath, string destServerFolder);
    }
}