using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Exceptions;
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
        public CancellationTokenSource CancellationTokenSource { get => _cancellationTokenSource; }

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
            // call GetSpacesListAsync to retrieve a spaces list and check required space authentication method
            // than open required session

            _output.WriteInfo("Opening session...");
            SpaceEnumerationItem desiredSpace = null;
            try
            {
                var spacesListResult = await _apiClient.GetSpacesListAsync(_cancellationTokenSource.Token);
                desiredSpace = spacesListResult.Items.FirstOrDefault(x => x.SpaceName.Equals(parameters.SpaceName, StringComparison.OrdinalIgnoreCase));
            }
            catch (MorphApiForbiddenException)
            {
                // space listing is forbidden.
                // tring to lookup
                var lookupResonse = await _apiClient.SpacesLookupAsync(new SpacesLookupRequest() { SpaceNames = { parameters.SpaceName } }, _cancellationTokenSource.Token);
                var lookup = lookupResonse.Values.First();
                if (lookup.Error != null)
                {
                    throw new Exception($"Space lookup failed. {lookup.Error.Message}");
                }
                desiredSpace = lookup.Data;
            }
            if (desiredSpace == null)
            {
                throw new Exception($"Server has no space '{parameters.SpaceName}'");
            }
            switch (desiredSpace.SpaceAccessRestriction)
            {
                case SpaceAccessRestriction.None:
                    _output.WriteInfo("Method: Anonymous access");
                    break;
                case SpaceAccessRestriction.BasicPassword when string.IsNullOrWhiteSpace(parameters.Password):
                    _output.WriteInfo("Method: Password protected space");
                    throw new Exception($"Space '{parameters.SpaceName}' requires password. You should pass '-password' parameter");
                case SpaceAccessRestriction.BasicPassword:
                    _output.WriteInfo("Method: Password protected space");
                    break;
                case SpaceAccessRestriction.WindowsAuthentication:
                    _output.WriteInfo("Method: Windows authentication");
                    break;
                case SpaceAccessRestriction.NotSupported when desiredSpace.IsPublic:
                    _output.WriteInfo("Method: Fall-back anonymous session");
                    break;
                case SpaceAccessRestriction.NotSupported when !desiredSpace.IsPublic & string.IsNullOrWhiteSpace(parameters.Password):
                    throw new Exception($"Space '{parameters.SpaceName}' requires password. You should pass '-password' parameter");
                case SpaceAccessRestriction.NotSupported when !desiredSpace.IsPublic:
                    _output.WriteInfo("Method: Fall-back password protected space");
                    break;
                default:
                    throw new Exception("Space access restriction method is not supported by this client.");
            }

            var session = await _apiClient.OpenSessionAsync(new OpenSessionRequest { SpaceName = parameters.SpaceName, Password = parameters.Password }, _cancellationTokenSource.Token);
            _output.WriteInfo("Session opened");
            return session;
        }
    }

}

