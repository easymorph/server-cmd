using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model;
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

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Target))
            {
                throw new WrongCommandFormatException("Target is required");
            }

            using (var apiSession = await OpenSession(parameters))
            {
                _output.WriteInfo(string.Format("Deleting file {0} in space {1}...", parameters.Target, parameters.SpaceName));
                await _apiClient.SpaceDeleteFileAsync(apiSession, parameters.SpaceName, parameters.Target, _cancellationTokenSource.Token);
                _output.WriteInfo("Operation completed");
            }

        }
    }
}
