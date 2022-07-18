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
            _output.WriteInfo("HttpSecurity: " + _apiClient.HttpSecurity);

            using (var apiSession = await OpenSession(parameters))
            {

                _output.WriteInfo("HttpSecurity: " + _apiClient.HttpSecurity);
                
                
          
                _output.WriteInfo("done");
            }

        }
    }
}
