using MorphCmd.Interfaces;
using MorphCmd.Models;
using MorphCmd.Utils;
using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Events;
using Morph.Server.Sdk.Exceptions;
using Morph.Server.Sdk.Model;
using Morph.Server.Sdk.Model.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic
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
            _output.WriteInfo("Attempting to start task " + parameters.TaskId.Value.ToString("D"));
            var info = await _apiClient.StartTaskAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token);
            //todo: get project name from StartTaskAsync
            _output.WriteInfo(string.Format("Project '{0}' is running. Waiting until done.", info.ProjectName));
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            while ((await _apiClient.GetRunningTaskStatusAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token)).IsRunning);
            _output.WriteInfo(string.Format("\nTask {0} completed", parameters.TaskId.Value.ToString("D")));
        }


        protected async Task RunTaskAndForget(Parameters parameters)
        {
            if (parameters.TaskId == null)
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("RUNASYNC usage sample: ems-cmd run http://10.20.30.40:6330 -space Default -taskID 8de5b50a-2d65-44f4-9e86-660c2408fb06");
                return;
            }
            _output.WriteInfo("Attempting to start task " + parameters.TaskId.Value.ToString("D"));
            var info = await _apiClient.StartTaskAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token);
            //todo: get project name from StartTaskAsync
            _output.WriteInfo(string.Format("Project '{0}' is running.", info.ProjectName));

        }

        protected async Task ValidateTasks(Parameters parameters)
        {
          
            try
            {
                _output.WriteInfo("Validating tasks for the project '" + parameters.Location + "'");                
                var result = await _apiClient.ValidateTasksAsync(parameters.Space, parameters.Location, _cancellationTokenSource.Token);

                if (result.FailedTasks.Count == 0)
                {
                    _output.WriteInfo("All tasks are valid");
                }
                else
                {
                    _output.WriteError(result.Message);
                    foreach(var item in result.FailedTasks)
                    {
                        _output.WriteInfo(item.TaskId + ": " + item.Message + "@" +item.TaskApiUrl);
                    }
                }
            }
            
            catch (MorphApiBadArgumentException ba)
            {
                _output.WriteError(ba.Message);
                foreach (var e in ba.Details)
                {
                    _output.WriteInfo(e.Field + ": " + e.Message);
                }
            }
        }

        protected async Task Browse(Parameters parameters)
        {

            if (string.IsNullOrWhiteSpace(parameters.Location))
            {
                _output.WriteInfo("Browsing the root folder of the space " + parameters.Space);
            }
            else
            {
                _output.WriteInfo("Browsing the folder '" + parameters.Location + "' of the space " + parameters.Space);
            }
            var data = await _apiClient.BrowseSpaceAsync(parameters.Space, parameters.Location, _cancellationTokenSource.Token);
            _output.WriteInfo("Space: " + data.SpaceName);
            _output.WriteInfo("Free space: " + data.FreeSpaceBytes + " bytes");
            foreach (var folder in data.Folders)
            {
                _output.WriteInfo(string.Format("{0}{1} {2}", folder.LastModified.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt").PadRight(30), "<DIR>".PadRight(16), folder.Name));
            }
            foreach (var file in data.Files)
            {
                _output.WriteInfo(string.Format("{0}{1} {2}", file.LastModified.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt").PadRight(30), file.FileSizeBytes.ToString("n0").PadLeft(16), file.Name));
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
                _output.WriteInfo(string.Format("Uploading file '{0}' to the root folder of space '{1}'...", parameters.From, parameters.Space ?? "Default"));
            }
            else
            {
                _output.WriteInfo(string.Format("Uploading file '{0}' to folder '{1}' of space '{2}'...", parameters.From, parameters.To, parameters.Space ?? "Default"));
            }

            if (!System.IO.File.Exists(parameters.From))
            {
                _output.WriteError(string.Format("File '{0} not exists.'", parameters.From));
                return;
            }



            ProgressBar progress = null;
            _apiClient.FileProgress += (object sender, FileEventArgs e) =>
            {
                if (e.State == FileProgressState.Starting)
                {
                    progress = new ProgressBar(_output, 40);
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

            var browsing = await _apiClient.BrowseSpaceAsync(parameters.Space, parameters.To, _cancellationTokenSource.Token);
            if (!browsing.CanUploadFiles)
                throw new Exception("Uploading to this space is disabled");


            if (parameters.YesToAll)
            {
                // don't care if file exists. 
                _output.WriteInfo(string.Format("YES key was passed. File will be overridden if it already exists"));
                await _apiClient.UploadFileAsync(parameters.Space, parameters.From, parameters.To, _cancellationTokenSource.Token, overwriteFileifExists: true);
            }
            else
            {

                var fileExists = browsing.FileExists(Path.GetFileName(parameters.From));
                if (fileExists)
                {
                    if (_output.IsOutputRedirected)
                    {
                        _output.WriteError(string.Format("Unable to upload file '{0}' due to file already exists. Use /y to override it ", parameters.To));
                    }
                    else
                    {
                        _output.WriteInfo("Uploading file already exists. Would you like to override it? Y/N");
                        _output.WriteInfo("You may pass /y parameter to override file without any questions");
                        var answer = _input.ReadLine();
                        if (answer.Trim().ToLowerInvariant().StartsWith("y"))
                        {
                            _output.WriteInfo("Uploading file...");
                            await _apiClient.UploadFileAsync(parameters.Space, parameters.From, parameters.To, _cancellationTokenSource.Token, overwriteFileifExists: true);
                            _output.WriteInfo("Operation complete");
                        }
                        else
                        {
                            _output.WriteInfo("Operation cancelled");
                        }
                    }
                }
                else
                {
                    await _apiClient.UploadFileAsync(parameters.Space, parameters.From, parameters.To, _cancellationTokenSource.Token, overwriteFileifExists: false);
                }




            }
        }


        protected async Task DeleteFile(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.File))
            {
                _output.WriteError("Wrong command format");
                Console.WriteLine("DEL usage sample: ems-cmd del http://10.20.30.40:6330 -space Default -location \"folder 1\" -file \"sample.txt\" ");
                return;
            }
            await _apiClient.DeleteFileAsync(parameters.Space, parameters.Location, parameters.File, _cancellationTokenSource.Token);

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
                            _output.WriteInfo(string.Format("Destination file '{0}' already exists. Would you like to override it? Y/N", destFileName));
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
            if (paramsDict.ContainsKey("file"))
                parameters.File = paramsDict["file"];
            if (paramsDict.ContainsKey("y"))
                parameters.YesToAll = true;
            if (paramsDict.ContainsKey("taskid"))
            {
                Guid guid;
                if (!Guid.TryParse(paramsDict["taskid"], out guid))
                {
                    _output.WriteError("The taskId is malformed. Expected: GUID like '8de5b50a-2d65-44f4-9e86-660c2408fb06'");
                    return null;
                }
                parameters.TaskId = guid;

            }
            parameters.Command = command.Trim().ToLowerInvariant();
            return parameters;
        }


        public async Task<bool> Handle(string command, Dictionary<string, string> paramsDict)
        {
            var parameters = ExtractParameters(command, paramsDict);
            if (parameters == null)
            {
                _output.WriteError("Unable to extract parameters. Exiting...");
                return false;
            }

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
                    case "del":
                        await DeleteFile(parameters);
                        break;
                    case "download":
                        await DownloadFile(parameters);
                        break;
                    case "validatetasks":
                        await ValidateTasks(parameters);
                        break;
                    default:
                        _output.WriteInfo("Supported commands: run, runasync, status, upload, browse, download");
                        break;

                }
                return true;
            }
            catch (AggregateException agr)
            {
                foreach (var e in agr.InnerExceptions)
                {
                    _output.WriteError(e.Message);
                }
                return false;
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException;
                if (inner != null)
                {
                    while (inner.InnerException != null)
                    {
                        inner = inner.InnerException;
                    }
                }
                _output.WriteError(ex.Message);
                if (inner != null)
                {
                    _output.WriteError(inner.Message);
                }
                return false;
            }


        }














    }
}
