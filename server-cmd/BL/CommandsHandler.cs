using MorphCmd.Interfaces;
using MorphCmd.Models;
using MorphCmd.Utils;
using MorphSDK.Client;
using MorphSDK.Events;
using MorphSDK.Exceptions;
using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MorphCmd.BL
{

    internal class CommandsHandler
    {
        private readonly IOutputEndpoint _output;
        private readonly IInputEndpoint _input;
        protected readonly IMorphServerApiClient _apiClient;
        private readonly CancellationTokenSource _cancellationTokenSource;


        public CommandsHandler(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient)
        {
            _output = output;
            _input = input;
            _apiClient = apiClient;
            _cancellationTokenSource = new CancellationTokenSource();

        }


        protected async Task RunTaskAndWait(Parameters parameters)
        {
            if (parameters.TaskId == null)
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("RUN usage sample: ems-cmd run http://10.20.30.40:6330 -space Default -taskID 8de5b50a-2d65-44f4-9e86-660c2408fb06");
                return;
            }
            _output.WriteInfo("Starting task " + parameters.TaskId.Value.ToString("D") + " in the space ..." + parameters.Space);
            await _apiClient.StartTaskAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token);
            _output.WriteInfo("Task started. Waiting until done.");
            do
            {
                _output.WriteSymbols(".");
                await Task.Delay(TimeSpan.FromSeconds(1));

            }
            while ((await _apiClient.GetRunningTaskStatusAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token)).IsRunning);
            _output.WriteInfo(string.Format("\nTask '{0}' completed", parameters.TaskId.Value.ToString("D")));
        }


        protected async Task RunTaskAndForget(Parameters parameters)
        {
            if (parameters.TaskId == null)
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("RUNASYNC usage sample: ems-cmd run http://10.20.30.40:6330 -space Default -taskID 8de5b50a-2d65-44f4-9e86-660c2408fb06");
                return;
            }
            _output.WriteInfo("Starting task " + parameters.TaskId.Value.ToString("D") + " in the space ..." + parameters.Space);
            await _apiClient.StartTaskAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token);
            _output.WriteInfo("Task started. Details are available in the Task log");

        }

        protected async Task Browse(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Location))
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("BROWSE usage sample: ems-cmd browse http://10.20.30.40:6330 -space Default -location  \"\\\" ");
                return;
            }
            _output.WriteInfo("Browsing folder " + parameters.Location + " in the space ..." + parameters.Space);
            var data = await _apiClient.BrowseSpace(parameters.Space, parameters.Location, _cancellationTokenSource.Token);
            foreach (var folder in data.Folders)
            {
                _output.WriteInfo(string.Format("FOLDER '{0}'", folder.Name));
            }
            foreach (var file in data.Files)
            {
                _output.WriteInfo(string.Format("FILE '{0}'; SIZE {1}", file.Name, file.FileSizeBytes));
            }
            _output.WriteInfo("Listing done");


        }

        protected async Task ServerStatus(Parameters parameters)
        {
            _output.WriteInfo("Retrieving server status...");
            var status = await _apiClient.GetServerStatusAsync(_cancellationTokenSource.Token);
            _output.WriteInfo("STATUS:");
            _output.WriteInfo(string.Format("StatusCode: {0}\nStatusMessage: {1}\nServerVersion:{2}", status.StatusCode, status.StatusMessage, status.Version.ToString()));

        }

        protected async Task UploadFile(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.From))
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("UPLOAD usage sample: ems-cmd upload http://10.20.30.40:6330 -space Default  -from \"C:\\Users\\Public\\Documents\\Morphs\\sample.morph\" -to \"folder 1\"");
                return;
            }

            if (string.IsNullOrWhiteSpace(parameters.To))
            {
                _output.WriteInfo("Parameter -to was omited. Using default value '/'");
            }

            _output.WriteInfo(string.Format("Uploading file '{0}' to folder '{1}' in a space '{2}'...", parameters.From, parameters.To, parameters.Space));
            if (!System.IO.File.Exists(parameters.From))
            {
                _output.WriteError(string.Format("File '{0} not exists.'", parameters.From));
                return;
            }



            ProgressBar progress = null;
            _apiClient.FileProgress += (object sender, FileEventArgs e) =>
            {
                if (e.State == FileProgressState.Starting)
                {   progress = new ProgressBar(_output, 40);
                    progress.Init();
                }
                else if (e.State == FileProgressState.Processing)
                {
                    progress.Report(e.Percent / 100);
                }
                else if (e.State == FileProgressState.Finishing)
                {
                    progress.Dispose();
                    progress = null;
                }

            };


            if (parameters.YesToAll)
            {
                // don't care if file exists. 
                _output.WriteInfo(string.Format("YES key was passed. File will be overridden if it already exists"));
                await _apiClient.UpdateFileAsync(parameters.Space, parameters.From, parameters.To, _cancellationTokenSource.Token);
            }
            else
            {
                try
                {
                    await _apiClient.UploadNewFileAsync(parameters.Space, parameters.From, parameters.To, _cancellationTokenSource.Token);
                }
                catch (MorphApiConflictException conflict)
                {
                    if (_output.IsOutputRedirected)
                    {
                        _output.WriteError(string.Format("Unable to upload file '{0}' due to {1}. Use /y to override it ", parameters.To, conflict.Message));
                    }
                    else
                    {
                        _output.WriteInfo("Uploading file already exists. Would you like to override it? (Y)es/No");
                        _output.WriteInfo("You may pass /y parameter to override file without any questions");
                        var answer = _input.ReadLine();
                        if (answer.Trim().ToLowerInvariant().StartsWith("y"))
                        {
                            _output.WriteInfo("Overriding file...");
                            await _apiClient.UpdateFileAsync(parameters.Space, parameters.From, parameters.To, _cancellationTokenSource.Token);
                            _output.WriteInfo("Operation complete");
                        }
                        else
                        {
                            _output.WriteInfo("Operation cancelled");
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
        }

        protected async Task DownloadFile(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.To) || string.IsNullOrWhiteSpace(parameters.From) || string.IsNullOrWhiteSpace(parameters.Space))
            {
                _output.WriteError("Wrong command format");
                Console.WriteLine("DOWNLOAD usage sample: ems-cmd download http://10.20.30.40:6330 -space Default -from \"folder 1\\file.map\"  -to \"C:\\Users\\Public\\Documents\\Morphs\"");
                return;
            }


            if (!System.IO.Directory.Exists(parameters.To))
            {
                _output.WriteError(string.Format("Destination directory {0} not found", parameters.To));
                return;
            }

            _output.WriteInfo(string.Format("Downloading file '{0}' from space '{1}' into '{2}'...", parameters.From, parameters.Space, parameters.To));

            ProgressBar progress = new ProgressBar(_output, 40);
            _apiClient.FileProgress += (object sender, FileEventArgs e) =>
            {
                if (e.State == FileProgressState.Starting)
                {
                    progress.Init();
                }
                else if (e.State == FileProgressState.Processing)
                {
                    progress.Report(e.Percent / 100);
                }
                else if (e.State == FileProgressState.Finishing)
                {
                    progress.Dispose();
                    progress = null;
                }

            };

            var tempFile = Guid.NewGuid().ToString("D") + ".tmp";
            tempFile = Path.Combine(parameters.To, tempFile);

            string destFileName = null;
            var allowLoading = false;
            try
            {
                using (Stream streamToWriteTo = File.Open(tempFile, FileMode.Create))
                {
                    try
                    {
                        await _apiClient.DownloadFileAsync(parameters.Space, parameters.From, (fileInfo) =>
                        {
                            destFileName = Path.Combine(parameters.To, fileInfo.FileName);
                            bool allowOverride = parameters.YesToAll;
                            if (!allowOverride && System.IO.File.Exists(destFileName))
                                throw new FileExistsException("File already exists");
                            allowLoading = true;
                            return true;

                        }, streamToWriteTo, _cancellationTokenSource.Token);
                    }
                    catch (FileExistsException)
                    {
                        allowLoading = false;
                        if (!_output.IsOutputRedirected)
                        {
                            _output.WriteInfo(string.Format("Destination file '{0}' already exists. Would you like to override it? (Y)es/No", destFileName));
                            _output.WriteInfo("You may pass /y parameter to override file without any questions");
                            var answer = _input.ReadLine();
                            if (answer.Trim().ToLowerInvariant().StartsWith("y"))
                            {
                                allowLoading = true;
                                _output.WriteInfo("File will be overridden");
                            }
                            else
                            {
                                _output.WriteInfo("Operation canceled");
                                throw;
                            }
                        }
                        else
                        {
                            _output.WriteError("File already exists. To override file user /y flag");
                            allowLoading = false;
                        }
                        if (allowLoading)
                        {
                            _output.WriteInfo(string.Format("Downloading '{0}' ...", parameters.From));
                            await _apiClient.DownloadFileAsync(parameters.Space, parameters.From, (fileInfo) =>
                            {
                                return true;
                            }, streamToWriteTo, _cancellationTokenSource.Token);
                        }
                    }



                }


                if (allowLoading)
                {

                    if (File.Exists(destFileName))
                        File.Delete(destFileName);
                    File.Move(tempFile, destFileName);

                }

                _output.WriteInfo("Operation completed");

            }
            finally
            {
                //drop file
                if (tempFile != null)
                    File.Delete(tempFile);

            }

        }



        private Parameters ExtractParameters(string command, Dictionary<string, string> paramsDict)
        {
            Parameters parameters = new Parameters();
            if (paramsDict.ContainsKey("space"))
                parameters.Space = paramsDict["space"];
            if (paramsDict.ContainsKey("from"))
                parameters.From = paramsDict["from"];
            if (paramsDict.ContainsKey("to"))
                parameters.To = paramsDict["to"];
            if (paramsDict.ContainsKey("location"))
                parameters.Location = paramsDict["location"];
            if (paramsDict.ContainsKey("y"))
                parameters.YesToAll = true;
            if (paramsDict.ContainsKey("taskid"))
            {
                Guid guid;
                if (!Guid.TryParse(paramsDict["taskid"], out guid))
                {
                    _output.WriteError("The taskId is malformed. Expected: GUID like '-taskId 8de5b50a-2d65-44f4-9e86-660c2408fb06'");
                    return null;
                }
                parameters.TaskId = guid;

            }
            parameters.Command = command.Trim().ToLowerInvariant();
            return parameters;
        }


        public async Task Handle(string command, Dictionary<string, string> paramsDict)
        {
            var parameters = ExtractParameters(command, paramsDict);

            try
            {
                switch (parameters.Command)
                {
                    case "run":
                        await RunTaskAndWait(parameters);
                        break;
                    case "runasync":
                        await RunTaskAndForget(parameters);
                        break;
                    case "status":
                        await ServerStatus(parameters);
                        break;
                    case "upload":
                        await UploadFile(parameters);
                        break;
                    case "browse":
                        await Browse(parameters);
                        break;
                    case "download":
                        await DownloadFile(parameters);
                        break;

                }
            }
            catch (AggregateException agr)
            {
                foreach (var e in agr.InnerExceptions)
                {
                    _output.WriteError("An error occured " + e.Message);
                }
            }
            catch (Exception ex)
            {
                _output.WriteError("An error occured " + ex.Message);
            }


        }














    }
}
