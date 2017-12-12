using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{

    internal class SpaceStatusCommand : BaseCommand, ICommand
    {
        public SpaceStatusCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            _output.WriteInfo($"Checking space {parameters.SpaceName} status...");

            using (var apiSession = await OpenSession(parameters))
            {
                var data = await _apiClient.GetSpaceStatusAsync(apiSession,  _cancellationTokenSource.Token);
                _output.WriteInfo("Space: " + data.SpaceName);
                _output.WriteInfo("IsPublic: " + data.IsPublic);
                _output.WriteInfo("Permissions: " + string.Join(", ", data.SpacePermissions));                

                _output.WriteInfo("done");
            }

        }
    }
}
