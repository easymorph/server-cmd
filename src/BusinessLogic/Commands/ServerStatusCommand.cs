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
    internal class ServerStatusCommand : BaseCommand, ICommand
    {
        public ServerStatusCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => false;

        public async Task Execute(Parameters parameters)
        {
            _output.WriteInfo("Retrieving server status...");
            var status = await _apiClient.GetServerStatusAsync(_cancellationTokenSource.Token);
            _output.WriteInfo("STATUS:");
            _output.WriteInfo($"StatusCode: {status.StatusCode}");
            _output.WriteInfo($"StatusMessage: {status.StatusMessage}");
            _output.WriteInfo($"ServerVersion: {status.Version}");
            _output.WriteInfo($"InstanceRunId: {status.InstanceRunId}");

        }
    }
}
