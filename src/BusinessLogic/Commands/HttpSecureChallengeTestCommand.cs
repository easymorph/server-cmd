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

    internal class HttpSecureChallengeTestCommand : BaseCommand, ICommand
    {
        public HttpSecureChallengeTestCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            _output.WriteInfo($"Checking server secure challenge ...");
            _output.WriteInfo("HttpSecurityState: " + _apiClient.HttpSecurityState);

            var status = await _apiClient.GetServerStatusAsync(_cancellationTokenSource.Token);

            _output.WriteInfo("STATUS:");
            _output.WriteInfo($"StatusCode: {status.StatusCode}");
            _output.WriteInfo($"StatusMessage: {status.StatusMessage}");
            _output.WriteInfo($"ServerVersion: {status.Version}");
            _output.WriteInfo($"InstanceRunId: {status.InstanceRunId}");
            _output.WriteInfo("HttpSecurityState: " + _apiClient.HttpSecurityState);
            var status3 = await _apiClient.GetServerStatusAsync(_cancellationTokenSource.Token);

            _output.WriteInfo("HttpSecurityState: " + _apiClient.HttpSecurityState);
            
            
      
            _output.WriteInfo("done");
            

        }
    }
}
