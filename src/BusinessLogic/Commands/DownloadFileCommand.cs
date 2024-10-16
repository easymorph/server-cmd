﻿using Morph.Server.Sdk.Client;
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
    internal class DownloadFileCommand : BaseCommand, ICommand
    {
        public DownloadFileCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {

            if (string.IsNullOrWhiteSpace(parameters.Target))
            {
                throw new WrongCommandFormatException("Target is required");
            }


            if (string.IsNullOrWhiteSpace(parameters.Source))
            {
                throw new WrongCommandFormatException("Source is required");
            }
            
            if (!Directory.Exists(parameters.Target))
            {
                throw new Exception(string.Format("Target directory {0} not found", parameters.Target));
            }

            using (var apiSession = await OpenSession(parameters))
            {
                _output.WriteInfo(string.Format("Downloading file '{0}' from space '{1}' into '{2}'...", parameters.Source, parameters.SpaceName, parameters.Target));

                ProgressBar progress = new ProgressBar(_output, 40);
                _apiClient.OnDataDownloadProgress += (object sender, FileTransferProgressEventArgs e) =>
                {
                    if (e.State == FileProgressState.Starting)
                    {
                        progress.Init();
                    }
                    else if (e.State == FileProgressState.Processing)
                    {
                        if (e.Percent.HasValue)
                        {
                            progress.Report(e.Percent.Value / 100);
                        }
                    }
                    else if (e.State == FileProgressState.Finishing)
                    {
                        progress.Dispose();
                        progress = null;
                    }
                    else if (e.State == FileProgressState.Cancelled)
                    {                        
                        progress.Dispose();
                        progress = null;
                        _output.WriteError("File download canceled.");
                    }

                };

                var tempFile = Guid.NewGuid().ToString("D") + ".tmp";
                tempFile = Path.Combine(parameters.Target, tempFile);

                string destFileName = null;
                var allowLoading = false;
                try
                {
                    using (Stream streamToWriteTo = File.Open(tempFile, FileMode.Create))
                    {
                        try
                        {

                            using (var serverStreamingData = await _apiClient.SpaceOpenStreamingDataAsync(apiSession,
                                parameters.SpaceName,
                                parameters.Source, _cancellationTokenSource.Token))
                                using(var reader = new BinaryReader(serverStreamingData.Stream))
                            {

                                destFileName = Path.Combine(parameters.Target, serverStreamingData.FileName);

                                if (!parameters.YesToAll && File.Exists(destFileName))
                                    throw new FileExistsException("File already exists");
                                allowLoading = true;

                                await serverStreamingData.Stream.CopyToAsync(streamToWriteTo, 81920, _cancellationTokenSource.Token);
                            }

                        }
                        catch (FileExistsException)
                        {
                            allowLoading = false;
                            if (!_output.IsOutputRedirected)
                            {
                                _output.WriteInfo(string.Format("Target file '{0}' already exists. Would you like to overwrite it? Y/N", destFileName));
                                _output.WriteInfo("You may pass /y parameter to overwrite file without any questions");
                                var answer = _input.ReadLine();
                                if (answer.Trim().ToLowerInvariant().StartsWith("y"))
                                {
                                    allowLoading = true;
                                    _output.WriteInfo("File will be overwritten");
                                }
                                else
                                {
                                    _output.WriteInfo("Operation canceled");
                                    throw new CommandFailedException();
                                }
                            }
                            else
                            {
                                _output.WriteError("File already exists. To overwrite file use /y flag");
                                allowLoading = false;
                            }
                            if (allowLoading)
                            {
                                _output.WriteInfo(string.Format("Downloading '{0}' ...", parameters.Source));
                                using (var serverStreamingData = await _apiClient.SpaceOpenStreamingDataAsync(apiSession,
                                    parameters.SpaceName,parameters.Source, _cancellationTokenSource.Token))
                                {
                                    await serverStreamingData.Stream.CopyToAsync(streamToWriteTo, 81920, _cancellationTokenSource.Token);
                                }
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
        }
    }
}
