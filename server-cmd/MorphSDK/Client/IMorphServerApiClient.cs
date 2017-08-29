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

        /// <summary>
        /// Download files from the server
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="remoteFolderPath">remote folder path</param>
        /// <param name="handleFile"></param>
        /// <param name="streamToWriteTo">stream to write to </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DownloadFileAsync(string spaceName, string remoteFolderPath, Func<DownloadFileInfo, bool> handleFile, Stream streamToWriteTo, CancellationToken cancellationToken);

        /// <summary>
        /// Download files from the server
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="remoteFolderPath">remote folder path</param>
        /// <param name="streamToWriteTo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DownloadFileInfo> DownloadFileAsync(string spaceName, string remoteFolderPath, Stream streamToWriteTo, CancellationToken cancellationToken);
        /// <summary>
        /// Returns running task status
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="taskId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<RunningTaskStatus> GetRunningTaskStatusAsync(string spaceName, Guid taskId, CancellationToken cancellationToken);
        /// <summary>
        /// Returns server status
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Starts specified task
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="taskId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<RunningTaskStatus> StartTaskAsync(string spaceName, Guid taskId, CancellationToken cancellationToken);

        Task StopTaskAsync(string spaceName, Guid taskId, CancellationToken cancellationToken);
        Task DeleteFileAsync(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken);
        /// <summary>
        /// Check if file extists
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="serverFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> IsFileExistsAsync(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Browse server folders 
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="folderPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SpaceBrowsingInfo> BrowseSpaceAsync(string spaceName, string folderPath, CancellationToken cancellationToken);
        /// <summary>
        /// Uploads stream to the server
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="inputStream">stream</param>
        /// <param name="fileName">file name</param>
        /// <param name="fileSize">file size</param>
        /// <param name="destFolderPath">destination path </param>
        /// <param name="cancellationToken"></param>
        /// <param name="overrideFileifExists"></param>
        /// <returns></returns>
        Task UploadFileAsync(string spaceName, Stream inputStream, string fileName, long fileSize, string destFolderPath, CancellationToken cancellationToken, bool overrideFileifExists = false);
        /// <summary>
        /// Uploads file to the server
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="localFilePath">path to local file</param>
        /// <param name="destFolderPath">destination path</param>
        /// <param name="cancellationToken"></param>
        /// <param name="overrideFileifExists"></param>
        /// <returns></returns>
        Task UploadFileAsync(string spaceName, string localFilePath, string destFolderPath, CancellationToken cancellationToken, bool overrideFileifExists = false);

        event EventHandler<FileEventArgs> FileProgress;
    }
}