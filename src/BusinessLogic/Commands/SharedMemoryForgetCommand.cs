using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class SharedMemoryForgetCommand : BaseCommand, ICommand
    {
        public SharedMemoryForgetCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(
            output, input, apiClient)
        {
        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Key))
                throw new WrongCommandFormatException("Key is required");

            using (var session = await OpenSession(parameters))
            {
                _output.WriteInfo($"Deleting shared memory record {parameters.Key} in space {parameters.SpaceName}...");
                var deletedKeyCount = await _apiClient.SharedMemoryForget(session, parameters.SpaceName, parameters.Key,
                    _cancellationTokenSource.Token);
                _output.WriteInfo($"Operation completed, {deletedKeyCount} records deleted");
            }
        }
    }
}