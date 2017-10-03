using Morph.Server.Sdk.Client;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class DeleteFileCommand : BaseCommand, ICommand
    {
        public DeleteFileCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Destination))
            {
                throw new WrongCommandFormatException("Destination is required");
            }

            _output.WriteInfo(string.Format("Deleting file {0} in space {1}...", parameters.Destination, parameters.Space ?? "Default"));
            await _apiClient.DeleteFileAsync(parameters.Space, parameters.Destination, null, _cancellationTokenSource.Token);
            _output.WriteInfo("Operation completed");

        }
    }
}
