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

namespace MorphSDK.Client
{
    public class MorphServerApiClient : IMorphServerApiClient
    {
        protected readonly Uri _apiHost;
        protected readonly string UserAgent = "MorphServerApiClient/0.1";
        protected HttpClient _httpClient;
        protected readonly string _api_v1 = "api/v1/";

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


        protected HttpClient ConstructHttpClient(Uri apiHost)
        {
            HttpClientHandler aHandler = new HttpClientHandler();
            aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;

            var client = new HttpClient(aHandler);
            client.BaseAddress = new Uri(apiHost, new Uri(_api_v1, UriKind.Relative));

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.DefaultRequestHeaders.Add("X-Client-Type", "EMS-CMD");
            client.MaxResponseContentBufferSize = 100 * 1024;
            client.Timeout = TimeSpan.FromMinutes(5);

            return client;
        }




        protected async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
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

                        throw new MorphClientGeneralException(errorResponse.error.code, errorResponse.error.message);
                    }
                    else throw new MorphClientCommunicationException("An error occurred while deserializing the response");
                }


            }
            else
            {
                //todo: analize response.StatusCode
                throw new MorphClientCommunicationException(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Start Task like "fire and forget"
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="taskId">tast guid</param>
        /// <returns></returns>
        public async Task StartTaskAsync(string spaceName, Guid taskId)
        {
            var url = "runningjobs/" + taskId.ToString("D");
            using (var response = await GetHttpClient().PostAsync(url, null))
            {
                await HandleResponse(response);
            }

        }

        /// <summary>
        /// Gets status of the task (Running/Not running) and payload
        /// </summary>
        /// <param name="spaceName">space name</param>
        /// <param name="taskId">task guid</param>
        /// <returns></returns>
        public async Task<RunningTaskStatus> GetRunningTaskStatusAsync(string spaceName, Guid taskId)
        {
            var url = "runningjobs/" + taskId.ToString("D");
            using (var response = await GetHttpClient().GetAsync(url))
            {
                var info = await HandleResponse<RunningTaskStatusDto>(response);
                return new RunningTaskStatus
                {
                    Id = Guid.Parse(info.Id),
                    IsRunning = info.IsRunning
                };
            }
        }

        /// <summary>
        /// Stops the Task
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task StopTaskAsync(string spaceName, Guid taskId)
        {
            var url = "runningjobs/" + taskId.ToString("D");
            using (var response = await GetHttpClient().DeleteAsync(url))
            {
                await HandleResponse(response);
            }
        }

        /// <summary>
        /// Resturns the server status
        /// </summary>
        /// <returns></returns>
        public async Task<ServerStatus> GetServerStatusAsync()
        {
            var url = "server/status";
            using (var response = await GetHttpClient().GetAsync(url))
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

        public async Task<DownloadFileInfo> DownloadFileAsync(string spaceName, string path, Stream streamToWriteTo)
        {
            var url = string.Format("space/{0}/files/{1}", spaceName, path);
            // it's necessary to add HttpCompletionOption.ResponseHeadersRead to disable caching
            using (HttpResponseMessage response = await GetHttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                if (response.IsSuccessStatusCode)
                {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        var contentDisposition = response.Content.Headers.ContentDisposition;
                        if (contentDisposition != null)
                        {
                            return new DownloadFileInfo
                            {
                                FileName = contentDisposition.FileName
                            };
                        }
                    }
                }
                else
                {
                    // TODO: check
                    await HandleErrorResponse(response);

                }
            return null;

        }

        public async Task UploadNewFileAsync(string spaceName, string filePath, string dest)
        {
            await InternalUploadFileAsync(spaceName, filePath, dest, overrideFile: false);
        }

        public async Task UpdateFileAsync(string spaceName, string filePath, string dest)
        {
            await InternalUploadFileAsync(spaceName, filePath, dest, overrideFile: true);
        }


        protected async Task InternalUploadFileAsync(string spaceName, string filePath, string dest, bool overrideFile)
        {
            string boundary = "EasyMorphCommandClient--------" + Guid.NewGuid().ToString("N");
            string url = string.Format("space/{0}/files/{1}", spaceName, dest);
            var cts = new CancellationTokenSource();
            using (var content = new MultipartFormDataContent(boundary))
            {
                FileStreamWithEvents fsSource = new FileStreamWithEvents(filePath, FileMode.Open, FileAccess.Read);

                using (fsSource)
                {
                    content.Add(new StreamContent(fsSource), "files", Path.GetFileName(filePath));
                    HttpRequestMessage requestMessage = new HttpRequestMessage()
                    {
                        Content = content,
                        Method = overrideFile ? HttpMethod.Post : HttpMethod.Put,
                        RequestUri = new Uri(url, UriKind.Relative)
                    };

                    using (var response = await GetHttpClient().SendAsync(requestMessage, cts.Token))
                    {
                        await HandleResponse(response);
                    }

                }
            }

        }
    }
}
