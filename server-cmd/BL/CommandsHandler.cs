using MorphCmd.Interfaces;
using MorphCmd.Models;
using MorphSDK.Client;
using MorphSDK.Exceptions;
using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MorphCmd.BL
{

    internal class CommandsHandler
    {
        private readonly IOutputEndpoint _output;
        private readonly IInputEndpoint _input;
        protected readonly IMorphServerApiClient _apiClient;

        public CommandsHandler(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient)
        {
            _output = output;
            _input = input;
            _apiClient = apiClient;
        }


        protected async Task RunAsync(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Space) || parameters.TaskId == null)
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("RUNASYNC usage sample: ems-cmd run http://10.20.30.40:6330 -space=Default -taskID=8de5b50a-2d65-44f4-9e86-660c2408fb06");
                return;
            }
            _output.WriteInfo("Starting task " + parameters.TaskId.Value.ToString("D") + " in the space ..." + parameters.Space);
            await _apiClient.StartTaskAsync(parameters.Space, parameters.TaskId.Value);
            _output.WriteInfo("Task started. Details are available in the Task log");

        }

        protected async Task ServerStatus(Parameters parameters)
        {
            _output.WriteInfo("Retrieving server status...");
            var status = await _apiClient.GetServerStatusAsync();
            _output.WriteInfo("STATUS:");
            _output.WriteInfo(string.Format("StatusCode: {0}\nStatusMessage: {1}\nServerVersion:{2}", status.StatusCode, status.StatusMessage, status.Version.ToString()));

        }

        protected async Task UploadFile(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.To) || string.IsNullOrWhiteSpace(parameters.From) || string.IsNullOrWhiteSpace(parameters.Space))
            {
                _output.WriteError("Wrong command format");
                _output.WriteInfo("UPLOAD usage sample: ems-cmd upload http://10.20.30.40:6330 -space=Default \"-from=C:\\Users\\Public\\Documents\\Morphs\\sample.morph\" \"-to=folder 1\"");
                return;
            }

            _output.WriteInfo(string.Format("Uploading file '{0}' to folder '{1}' in a space '{2}'...", parameters.From, parameters.To, parameters.Space));
            if (!System.IO.File.Exists(parameters.From))
            {
                _output.WriteError(string.Format("File '{0} not exists.'", parameters.From));
                return;
            }


            //todo: check that dest folder exists before sending entire file content
            // e.g. use HEAD or list files and folders

            if (parameters.YesToAll)
            {
                // don't care if file exists. 
                _output.WriteInfo(string.Format("YES key was passed. File will be overridden if it already exists"));
                await _apiClient.UpdateFileAsync(parameters.Space, parameters.From, parameters.To);
            }
            else
            {
                try
                {
                    await _apiClient.UploadNewFileAsync(parameters.Space, parameters.From, parameters.To);
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
                            await _apiClient.UpdateFileAsync(parameters.Space, parameters.From, parameters.To);
                        }
                        else
                        {
                            _output.WriteInfo("Operation cancelled");
                        }
                    }
                }
            }
        }

        protected async Task DownloadFile(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.To) || string.IsNullOrWhiteSpace(parameters.From) || string.IsNullOrWhiteSpace(parameters.Space))
            {
                _output.WriteError("Wrong command format");
                Console.WriteLine("DOWNLOAD usage sample: ems-cmd download http://10.20.30.40:6330 -space=Default \"-from=folder 1\\file.map \" \"-to=C:\\Users\\Public\\Documents\\Morphs\"");
                return;
            }


            if (!System.IO.Directory.Exists(parameters.To))
            {
                _output.WriteError(string.Format("Destination directory {0} not found", parameters.To));
                return;
            }


            var tempFile = Guid.NewGuid().ToString("D") + ".tmp";
            tempFile = Path.Combine(parameters.To, tempFile);
            DownloadFileInfo dowloadFileInfo = null;

            try
            {
                using (Stream streamToWriteTo = File.Open(tempFile, FileMode.Create))
                {
                    dowloadFileInfo = await _apiClient.DownloadFileAsync(parameters.Space, parameters.From, streamToWriteTo);
                }
            }
            catch(Exception)
            {
                //drop file
                File.Delete(tempFile);
                _output.WriteError("An error occured while downloading file");
                throw;
            }

            if (dowloadFileInfo != null)
            {
                var destFileName = Path.Combine(parameters.To, dowloadFileInfo.FileName);
                bool allowOverride = parameters.YesToAll;
                if (!allowOverride && System.IO.File.Exists(destFileName))
                {
                    _output.WriteInfo(string.Format("Destination file '{0}' already exists. Would you like to override it? (Y)es/No", destFileName));
                    _output.WriteInfo("You may pass /y parameter to override file without any questions");
                    var answer = _input.ReadLine();
                    if (answer.Trim().ToLowerInvariant().StartsWith("y"))
                    {
                        allowOverride = true;
                        _output.WriteInfo("File will be overridden");
                    }
                    else
                    {
                        _output.WriteInfo("Operation canceled");
                        return;
                    }
                }
                else
                {
                    allowOverride = true;
                }
                
                if (allowOverride)
                {
                    if (File.Exists(destFileName))
                        File.Delete(destFileName);
                    File.Move(tempFile, destFileName);
                }              

            }

            _output.WriteInfo("Operation completed");

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
            if (paramsDict.ContainsKey("y"))
                parameters.YesToAll = true;
            if (paramsDict.ContainsKey("taskid"))
            {
                Guid guid;
                if (!Guid.TryParse(paramsDict["taskid"], out guid))
                {
                    _output.WriteError("The taskId is malformed. Expected: GUID like '-taskId=8de5b50a-2d65-44f4-9e86-660c2408fb06'");
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
                    case "runasync":
                        await RunAsync(parameters);
                        break;
                    case "status":
                        await ServerStatus(parameters);
                        break;
                    case "upload":
                        await UploadFile(parameters);
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
