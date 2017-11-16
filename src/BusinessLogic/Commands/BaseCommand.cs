using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal abstract class BaseCommand
    {


        protected readonly IOutputEndpoint _output;
        protected readonly IInputEndpoint _input;
        protected readonly IMorphServerApiClient _apiClient;
        protected readonly CancellationTokenSource _cancellationTokenSource;

        public BaseCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient)
        {
            _output = output;
            _input = input;
            _apiClient = apiClient;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected void RequireParam(Guid? value)
        {

        }

        protected async Task<ApiSession> OpenSession(Parameters parameters)
        {
            // for simplification we just check that the user has pass any password
            // in more complex logic, you should call GetSpacesListAsync to retrieve a spaces list and check the isPublic property or the space
            // isPublic  means that you need to open an anon session, otherwise - open a real session
            if (string.IsNullOrWhiteSpace(parameters.Password))
            {
                var apiSession = ApiSession.Anonymous(parameters.SpaceName);
                return apiSession;
            }
            else
            {
                var apiSession = await _apiClient.OpenSessionAsync(parameters.SpaceName, parameters.Password, _cancellationTokenSource.Token);
                return apiSession;
            }

        }

    }
}
