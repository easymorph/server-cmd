using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Events;
using Morph.Server.Sdk.Model;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using MorphCmd.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class UploadFileCommand : BaseCommand, ICommand
    {
        public UploadFileCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Source))
            {
                throw new WrongCommandFormatException("Source is required");
            }

           
            if (string.IsNullOrWhiteSpace(parameters.Target))
            {
                _output.WriteInfo(string.Format("Uploading file '{0}' to the root folder of space '{1}'...", parameters.Source, parameters.SpaceName));
            }
            else
            {
                _output.WriteInfo(string.Format("Uploading file '{0}' to folder '{1}' of space '{2}'...", parameters.Source, parameters.Target, parameters.SpaceName));
            }

            if (!File.Exists(parameters.Source))
            {
                throw new Exception(string.Format("File '{0} not exists.'", parameters.Source));
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
                else if(e.State == FileProgressState.Cancelled)
                {                    
                    progress.Dispose();
                    progress = null;
                    _output.WriteError("File upload canceled.");
                }
            };

            using (var apiSession = await OpenSession(parameters))
            {
                var browsing = await _apiClient.BrowseSpaceAsync(apiSession, parameters.Target, _cancellationTokenSource.Token);
                if (!browsing.CanUploadFiles)
                {
                    throw new Exception("Uploading to this space is disabled");
                }


                if (parameters.YesToAll)
                {
                    // don't care if file exists. 
                    _output.WriteInfo(string.Format("YES key was passed. File will be overridden if it already exists"));
                    await _apiClient.UploadFileAsync(apiSession, parameters.Source, parameters.Target, _cancellationTokenSource.Token, overwriteFileifExists: true);
                }
                else
                {

                    var fileExists = browsing.FileExists(Path.GetFileName(parameters.Source));
                    if (fileExists)
                    {
                        if (_output.IsOutputRedirected)
                        {
                            _output.WriteError(string.Format("Unable to upload file '{0}' due to file already exists. Use /y to override it ", parameters.Target));
                        }
                        else
                        {
                            _output.WriteInfo("Uploading file already exists. Would you like to override it? Y/N");
                            _output.WriteInfo("You may pass /y parameter to override file without any questions");
                            var answer = _input.ReadLine();
                            if (answer.Trim().ToLowerInvariant().StartsWith("y"))
                            {
                                _output.WriteInfo("Uploading file...");
                                await _apiClient.UploadFileAsync(apiSession, parameters.Source, parameters.Target, _cancellationTokenSource.Token, overwriteFileifExists: true);
                                _output.WriteInfo("Operation complete");
                            }
                            else
                            {
                                _output.WriteInfo("Operation canceled");
                            }
                        }
                    }
                    else
                    {
                        await _apiClient.UploadFileAsync(apiSession, parameters.Source, parameters.Target, _cancellationTokenSource.Token, overwriteFileifExists: false);
                    }

                }
            }
        }
    }
}
