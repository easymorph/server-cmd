using MorphSDK.Dto;
using MorphSDK.Exceptions;
using MorphSDK.Helper;
using MorphSDK.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using MorphSDK.Events;
using System.Collections.Specialized;

namespace MorphSDK.Client
{

    public class MorphServerApiClient : IMorphServerApiClient
    {
        protected readonly Uri _apiHost;
        protected readonly string UserAgent = "MorphServerApiClient/0.1";
        protected HttpClient _httpClient;
        protected readonly string _api_v1 = "api/v1/";
        protected readonly string _defaultSpaceName = "Default";

        public MorphServerApiClient(string apiHost)
        {
            if (!apiHost.EndsWith("/"))
                apiHost += "/";
            _apiHost = new Uri(apiHost);

        }

        protected HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                _httpClient = ConstructHttpClient(_apiHost);
            }
            return _httpClient;
        }


        public event EventHandler<FileEventArgs> FileProgress;


        protected HttpClient ConstructHttpClient(Uri apiHost)
        {
            HttpClientHandler aHandler = new HttpClientHandler();
            aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;

            var client = new HttpClient(aHandler);
            client.BaseAddress = new Uri(apiHost, new Uri(_api_v1, UriKind.Relative));

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                });
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.DefaultRequestHeaders.Add("X-Client-Type", "EMS-CMD");
            client.MaxResponseContentBufferSize = 100 * 1024;
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,                    
                    NoStore = true                    
                };
            


            client.Timeout = TimeSpan.FromMinutes(15);

            return client;
        }




        protected async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(T));
                var d = Encoding.UTF8.GetBytes(result);
                using (var ms = new MemoryStream(d))
                {
                    var data = (T)serializer.ReadObject(ms);
                    return data;
                }
            }
            else
            {
                await HandleErrorResponse(response);
                return default(T);

            }

        }

        protected async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
            }

        }

        private static async Task HandleErrorResponse(HttpResponseMessage response)
        {

            var result = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(result))
            {

                var serializer = new DataContractJsonSerializer(typeof(ErrorResponse));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                {
                    var errorResponse = (ErrorResponse)serializer.ReadObject(ms);
                    if (errorResponse != null && errorResponse.error != null)
                    {
                        switch (errorResponse.error.code)
                        {
                            case ReadableErrorTopCode.Conflict: throw new MorphApiConflictException(errorResponse.error.message);
                            case ReadableErrorTopCode.NotFound: throw new MorphApiNotFoundException(errorResponse.error.message);
                            case ReadableErrorTopCode.Forbidden: throw new MorphApiForbiddenException(errorResponse.error.message);
                        }

                        throw new MorphClientGeneralException(errorResponse.error.code, errorResponse.error.message);
                    }
                    else throw new MorphClientCommunicationException("An error occurred while deserializing the response");
                }


            }
            else
            {
                //todo: analize response.StatusCode
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Conflict: throw new MorphApiConflictException(response.ReasonPhrase ?? "Conflict");
                    case HttpStatusCode.NotFound: throw new MorphApiNotFoundException(response.ReasonPhrase ?? "Not found");
                    case HttpStatusCode.Forbidden: throw new MorphApiForbiddenException(response.ReasonPhrase ?? "Forbidden");
                }
                throw new MorphClientCommunicationException(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Start Task like "fire and forget"
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="taskId">tast guid</param>
        /// <returns></returns>
        public async Task<RunningTaskStatus> StartTaskAsync(string spaceName, Guid taskId, CancellationToken cancellationToken)
        {
            var url = "runningtasks/" + taskId.ToString("D");
            using (var response = await GetHttpClient().PostAsync(url, null, cancellationToken))
            {
                var info = await HandleResponse<RunningTaskStatusDto>(response);
                return new RunningTaskStatus
                {
                    Id = Guid.Parse(info.Id),
                    IsRunning = info.IsRunning,
                    ProjectName = info.ProjectName
                };
            }

        }

        /// <summary>
        /// Gets status of the task (Running/Not running) and payload
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="taskId">task guid</param>
        /// <returns></returns>
        public async Task<RunningTaskStatus> GetRunningTaskStatusAsync(string spaceName, Guid taskId, CancellationToken cancellationToken)
        {
            var nvc = new NameValueCollection();
            nvc.Add("_", DateTime.Now.Ticks.ToString());
            var url = string.Format("runningtasks/{0}{1}", taskId.ToString("D"), nvc.ToQueryString());

            using (var response = await GetHttpClient().GetAsync(url, cancellationToken))
            {
                var info = await HandleResponse<RunningTaskStatusDto>(response);
                return new RunningTaskStatus
                {
                    Id = Guid.Parse(info.Id),
                    IsRunning = info.IsRunning,
                    ProjectName = info.ProjectName
                };
            }
        }

        /// <summary>
        /// Stops the Task
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task StopTaskAsync(string spaceName, Guid taskId, CancellationToken cancellationToken)
        {
            var url = "runningtasks/" + taskId.ToString("D");
            using (var response = await GetHttpClient().DeleteAsync(url, cancellationToken))
            {
                await HandleResponse(response);
            }
        }

        /// <summary>
        /// Resturns the server status
        /// </summary>
        /// <returns></returns>
        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken)
        {
            var nvc = new NameValueCollection();
            nvc.Add("_", DateTime.Now.Ticks.ToString());

            var url = "server/status" + nvc.ToQueryString();
            using (var response = await GetHttpClient().GetAsync(url, cancellationToken))
            {
                var dto = await HandleResponse<ServerStatusDto>(response);
                return new ServerStatus
                {
                    StatusCode = dto.StatusCode,
                    StatusMessage = dto.StatusMessage,
                    Version = Version.Parse(dto.Version)
                };

            }
        }

        public async Task<DownloadFileInfo> DownloadFileAsync(string spaceName, string path, Stream streamToWriteTo, CancellationToken cancellationToken)
        {
            DownloadFileInfo fileInfo = null;
            await DownloadFileAsync(spaceName, path, (fi) => { fileInfo = fi; return true; }, streamToWriteTo, cancellationToken);
            return fileInfo;
        }

        public async Task DownloadFileAsync(string spaceName, string path, Func<DownloadFileInfo, bool> handleFile, Stream streamToWriteTo, CancellationToken cancellationToken)
        {
            var nvc = new NameValueCollection();
            nvc.Add("_", DateTime.Now.Ticks.ToString());
            path = PreparePath(path);
            var url = string.Format("space/{0}/files/{1}{2}", spaceName ?? _defaultSpaceName, path,nvc.ToQueryString());
            // it's necessary to add HttpCompletionOption.ResponseHeadersRead to disable caching
            using (HttpResponseMessage response = await GetHttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                if (response.IsSuccessStatusCode)
                {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        var contentDisposition = response.Content.Headers.ContentDisposition;
                        DownloadFileInfo dfi = null;
                        if (contentDisposition != null)
                        {
                            dfi = new DownloadFileInfo
                            {
                                FileName = contentDisposition.FileName
                            };
                        }
                        var contentLength = response.Content.Headers.ContentLength;
                        var fileProgress = new FileProgress(dfi.FileName, contentLength.Value);
                        fileProgress.StateChanged += DownloadProgress_StateChanged;

                        var bufferSize = 4096;
                        if (handleFile(dfi))
                        {

                            var buffer = new byte[bufferSize];
                            var size = contentLength.Value;
                            var processed = 0;
                            var lastUpdate = DateTime.MinValue;

                            fileProgress.ChangeState(FileProgressState.Starting);

                            while (true)
                            {
                                var length = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                                if (length <= 0) break;
                                await streamToWriteTo.WriteAsync(buffer, 0, length);
                                processed += length;
                                if (DateTime.Now - lastUpdate > TimeSpan.FromMilliseconds(250))
                                {
                                    fileProgress.SetProcessedBytes(processed);
                                    fileProgress.ChangeState(FileProgressState.Processing);
                                    lastUpdate = DateTime.Now;
                                }
                            }

                            fileProgress.ChangeState(FileProgressState.Finishing);

                        }

                    }
                }
                else
                {
                    // TODO: check
                    await HandleErrorResponse(response);

                }


        }

        public async Task UploadFileAsync(string spaceName, string filePath, string dest, CancellationToken cancellationToken)
        {
            await InternalUploadFileAsync(spaceName, filePath, dest, cancellationToken, overrideFile: false);
        }

        public async Task UpdateFileAsync(string spaceName, string filePath, string dest, CancellationToken cancellationToken)
        {
            await InternalUploadFileAsync(spaceName, filePath, dest, cancellationToken, overrideFile: true);
        }


        protected async Task InternalUploadFileAsync(string spaceName, string filePath, string dest, CancellationToken cancellationToken, bool overrideFile)
        {
            try
            {
                string boundary = "EasyMorphCommandClient--------" + Guid.NewGuid().ToString("N");
                dest = PreparePath(dest);
                string url = string.Format("space/{0}/files/{1}", spaceName ?? _defaultSpaceName, dest);
                //var cts = new CancellationTokenSource();
                using (var content = new MultipartFormDataContent(boundary))
                {
                    var fileSize = new System.IO.FileInfo(filePath).Length;
                    FileStream fsSource = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                    using (fsSource)
                    {
                        var downloadProgress = new FileProgress(filePath, fileSize);
                        downloadProgress.StateChanged += DownloadProgress_StateChanged;
                        using (var streamContent = new ProgressStreamContent(fsSource, downloadProgress))
                        {
                            content.Add(streamContent, "files", Path.GetFileName(filePath));
                            var requestMessage = new HttpRequestMessage()
                            {
                                Content = content,
                                Method = overrideFile ? HttpMethod.Put : HttpMethod.Post,
                                RequestUri = new Uri(url, UriKind.Relative)
                            };
                            using (requestMessage)
                            {
                                using (var response = await GetHttpClient().SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                                {
                                    await HandleResponse(response);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            when (ex.InnerException != null && ex.InnerException is WebException)
            {
                var einner = ex.InnerException as WebException;
                if (einner.Status == WebExceptionStatus.ConnectionClosed)
                    throw new MorphApiNotFoundException("Specified folder not found");

            }
        }

        private void DownloadProgress_StateChanged(object sender, FileEventArgs e)
        {
            if (FileProgress != null)
            {
                FileProgress(this, e);
            }

        }

        public async Task<SpaceBrowsingInfo> BrowseSpaceAsync(string spaceName, string folder, CancellationToken cancellationToken)
        {
            folder = PreparePath(folder);
            var nvc = new NameValueCollection();
            nvc.Add("_", DateTime.Now.Ticks.ToString());            

            var url = string.Format("space/{0}/browse/{1}{2}", spaceName ?? _defaultSpaceName, folder, nvc.ToQueryString());
            using (var response = await GetHttpClient().GetAsync(url, cancellationToken))
            {
                var dto = await HandleResponse<SpaceBrowsingResponseDto>(response);
                return SpaceBrowsingMapper.MapFromDto(dto);

            }
        }

        public async Task<bool> IsFileExistsAsync(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken)
        {
            serverFolder = PreparePath(serverFolder);
            string path = PreparePath(serverFolder + "/" + fileName);
            var url = string.Format("space/{0}/files/{1}", spaceName ?? _defaultSpaceName, path);
            try
            {
                using (var requestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Head,
                    RequestUri = new Uri(url, UriKind.Relative)
                })
                using (HttpResponseMessage response = await GetHttpClient().SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    await HandleResponse(response);
                }
            }
            catch (MorphApiNotFoundException)
            {
                return false;
            }

            return true;
        }


        private string PreparePath(string urlPath)
        {
            return urlPath?.Replace('\\', '/')?.Trim('/');
        }

        public async Task DeleteFileAsync(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken)
        {
            serverFolder = PreparePath(serverFolder);
            string path = PreparePath(serverFolder + "/" + fileName);
            var url = string.Format("space/{0}/files/{1}", spaceName ?? _defaultSpaceName, path);

            using (HttpResponseMessage response = await GetHttpClient().DeleteAsync(url, cancellationToken))
            {
                await HandleResponse(response);
            }

        }
    }


}
