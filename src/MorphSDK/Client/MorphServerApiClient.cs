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
using MorphSDK.Dto.Commands;
using MorphSDK.Model.Errors;
using MorphSDK.Mappers;

namespace MorphSDK.Client
{

    public class MorphServerApiClient : IMorphServerApiClient
    {
        protected readonly Uri _apiHost;
        protected readonly string UserAgent = "MorphServerApiClient/0.1";
        protected HttpClient _httpClient;
        protected readonly string _api_v1 = "api/v1/";
        protected readonly string _defaultSpaceName = "default";

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
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializationHelper.Deserialize<T>(content);
                return result;
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

            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                var errorResponse = JsonSerializationHelper.Deserialize<ErrorResponse>(content);
                if (errorResponse == null)
                    throw new MorphClientCommunicationException("An error occurred while deserializing the response");

                var serializer = new DataContractJsonSerializer(typeof(ErrorResponse));
                if (errorResponse.error != null)
                {
                    switch (errorResponse.error.code)
                    {
                        case ReadableErrorTopCode.Conflict: throw new MorphApiConflictException(errorResponse.error.message);
                        case ReadableErrorTopCode.NotFound: throw new MorphApiNotFoundException(errorResponse.error.message);
                        case ReadableErrorTopCode.Forbidden: throw new MorphApiForbiddenException(errorResponse.error.message);
                        case ReadableErrorTopCode.BadArgument: throw new MorphApiBadArgumentException(FieldErrorsMapper.MapFromDto(errorResponse.error), errorResponse.error.message); 
                        case ReadableErrorTopCode.CommandFailed:
                            {
                                switch (errorResponse.error.innererror.code)
                                {
                                    case "ValidateTasksError":
                                        var validateTasksError = (ValidateTasksErrorDto)errorResponse.error.innererror;
                                        throw new MorphApiCommandFailedException<ValidateTasksError>(ValidateTasksErrorMapper.MapFromDto(validateTasksError), errorResponse.error.message);                                        
                                    default: throw new NotImplementedException();
                                }
                                
                            }

                    }

                    throw new MorphClientGeneralException(errorResponse.error.code, errorResponse.error.message);
                }
                else throw new MorphClientCommunicationException("An error occurred while deserializing the response");
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
            spaceName = PrepareSpaceName(spaceName);
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
            spaceName = PrepareSpaceName(spaceName);
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
            spaceName = PrepareSpaceName(spaceName);
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

        public async Task<DownloadFileInfo> DownloadFileAsync(string spaceName, string remoteFolderPath, Stream streamToWriteTo, CancellationToken cancellationToken)
        {
            DownloadFileInfo fileInfo = null;
            await DownloadFileAsync(spaceName, remoteFolderPath, (fi) => { fileInfo = fi; return true; }, streamToWriteTo, cancellationToken);
            return fileInfo;
        }

        public async Task DownloadFileAsync(string spaceName, string remoteFolderPath, Func<DownloadFileInfo, bool> handleFile, Stream streamToWriteTo, CancellationToken cancellationToken)
        {
            spaceName = PrepareSpaceName(spaceName);
            var nvc = new NameValueCollection();
            nvc.Add("_", DateTime.Now.Ticks.ToString());
            var url = JoinUrl("space", spaceName, "files", remoteFolderPath) + nvc.ToQueryString();
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


        public async Task UploadFileAsync(string spaceName, string localFilePath, string destFolderPath, CancellationToken cancellationToken, bool overrideFileifExists = false)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException(string.Format("File '{0}' not found", localFilePath));
            var fileSize = new System.IO.FileInfo(localFilePath).Length;
            var fileName = Path.GetFileName(localFilePath);
            using (var fsSource = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                await UploadFileAsync(spaceName, fsSource, fileName, fileSize, destFolderPath, cancellationToken, overrideFileifExists);
                return;
            }

        }

        public async Task UploadFileAsync(string spaceName, Stream inputStream, string fileName, long fileSize, string destFolderPath, CancellationToken cancellationToken, bool overrideFileifExists = false)
        {
            try
            {
                spaceName = PrepareSpaceName(spaceName);
                string boundary = "EasyMorphCommandClient--------" + Guid.NewGuid().ToString("N");
                string url = JoinUrl("space", spaceName, "files", destFolderPath);

                using (var content = new MultipartFormDataContent(boundary))
                {
                    var downloadProgress = new FileProgress(fileName, fileSize);
                    downloadProgress.StateChanged += DownloadProgress_StateChanged;
                    using (var streamContent = new ProgressStreamContent(inputStream, downloadProgress))
                    {
                        content.Add(streamContent, "files", Path.GetFileName(fileName));
                        var requestMessage = new HttpRequestMessage()
                        {
                            Content = content,
                            Method = overrideFileifExists ? HttpMethod.Put : HttpMethod.Post,
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

        public async Task<SpaceBrowsingInfo> BrowseSpaceAsync(string spaceName, string folderPath, CancellationToken cancellationToken)
        {
            spaceName = PrepareSpaceName(spaceName);
            var nvc = new NameValueCollection();
            nvc.Add("_", DateTime.Now.Ticks.ToString());

            var url = JoinUrl("space", spaceName, "browse", folderPath) + nvc.ToQueryString();
            using (var response = await GetHttpClient().GetAsync(url, cancellationToken))
            {
                var dto = await HandleResponse<SpaceBrowsingResponseDto>(response);
                return SpaceBrowsingMapper.MapFromDto(dto);

            }
        }

        public async Task<bool> IsFileExistsAsync(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken)
        {
            spaceName = PrepareSpaceName(spaceName);
            var url = JoinUrl("space", spaceName, "files", serverFolder, fileName);
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



        private string PrepareSpaceName(string spaceName)
        {
            return string.IsNullOrWhiteSpace(spaceName) ? _defaultSpaceName : spaceName.ToLower();
        }

        private string JoinUrl(params string[] urls)
        {
            var result = string.Empty;
            for (var i = 0; i < urls.Length; i++)
            {
                var p = urls[i];
                if (p == null)
                    continue;

                p = p.Replace('\\', '/');
                p = p.Trim(new[] { '/' });
                if (string.IsNullOrWhiteSpace(p))
                    continue;

                if (result != string.Empty)
                    result += "/";
                result += p;

            }
            return result;
        }

        public async Task DeleteFileAsync(string spaceName, string serverFolder, string fileName, CancellationToken cancellationToken)
        {

            spaceName = PrepareSpaceName(spaceName);
            var url = JoinUrl("space", spaceName, "files", serverFolder, fileName);

            using (HttpResponseMessage response = await GetHttpClient().DeleteAsync(url, cancellationToken))
            {
                await HandleResponse(response);
            }

        }


        /// <summary>
        /// Validate tasks. Checks that there are no excess parameters in the created tasks. Raises <see cref="MorphSDK.Exceptions.MorphApiCommandFailedException{MorphSDK.Model.Errors.ValidateTasksError}"/> 
        /// where T is <see cref="MorphSDK.Model.Errors.ValidateTasksError"/> in case of validation errors
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="projectPath">path to the morph project file</param>
        /// <exception cref="MorphSDK.Exceptions.MorphApiCommandFailedException{TDetail}">Thrown when validation fails. See more info in exception Details property</exception>
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="MorphSDK.Model.Errors.ValidateTasksError"/>
        /// </para>
        /// <exception cref="MorphSDK.Exceptions.MorphApiBadArgumentException">Thrown when bad arguments were passed</exception>
        /// <returns></returns>
        public async Task ValidateTasksAsync(string spaceName, string projectPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException(nameof(projectPath));
            spaceName = PrepareSpaceName(spaceName);
            var url = "commands/validatetasks";
            var request = new ValidateTasksRequestDto
            {
                SpaceName = spaceName,
                ProjectPath = projectPath
            };
            using (var response = await GetHttpClient().PostAsync(url, new StringContent(JsonSerializationHelper.Serialize(request), Encoding.UTF8, "application/json"), cancellationToken))
            {

                await HandleResponse(response);
             
            }
        }
    }


}
