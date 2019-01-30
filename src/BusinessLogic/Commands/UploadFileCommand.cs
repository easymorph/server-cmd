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
            _apiClient.OnFileUploadProgress += (object sender, FileTransferProgressEventArgs e) =>
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
                var spaceBrowsing = await _apiClient.SpaceBrowseAsync(apiSession, parameters.Target, _cancellationTokenSource.Token); 
                var spaceStatus = await _apiClient.GetSpaceStatusAsync(apiSession, _cancellationTokenSource.Token);
                if (!spaceStatus.UserPermissions.Contains(UserSpacePermission.FileUpload))
                {
                    throw new Exception("Uploading to this space is disabled");
                }

                var transferUtility = new DataTransferUtility(_apiClient, apiSession);

                if (parameters.YesToAll)
                {
                    // don't care if file exists. 
                    _output.WriteInfo(string.Format("YES key was passed. File will be overridden if it already exists"));
                    await transferUtility.SpaceUploadFileAsync(parameters.Source, parameters.Target, _cancellationTokenSource.Token, overwriteExistingFile: true);
                }
                else
                {

                    var fileExists = spaceBrowsing.FileExists(Path.GetFileName(parameters.Source));
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
                                await transferUtility.SpaceUploadFileAsync(parameters.Source, parameters.Target, _cancellationTokenSource.Token, overwriteExistingFile: true);
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
                        await transferUtility.SpaceUploadFileAsync(parameters.Source, parameters.Target, _cancellationTokenSource.Token, overwriteExistingFile: false);
                    }

                }
            }
        }
    }
}
