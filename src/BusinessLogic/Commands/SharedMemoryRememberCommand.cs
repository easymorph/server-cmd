using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model.SharedMemory;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class SharedMemoryRememberCommand : BaseCommand, ICommand
    {
        public SharedMemoryRememberCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(
            output, input, apiClient)
        {
        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Key))
                throw new WrongCommandFormatException("Key is required");
            if (string.IsNullOrWhiteSpace(parameters.Value))
                throw new WrongCommandFormatException("Value is required");

            using (var session = await OpenSession(parameters))
            {
                _output.WriteInfo($"Saving shared memory record {parameters.Key} in space {parameters.SpaceName}...");
                _ = await _apiClient.SharedMemoryRemember(
                    session,
                    parameters.SpaceName,
                    parameters.Key,
                    SharedMemoryValue.NewText(parameters.Value),
                    OverwriteBehavior.Overwrite,
                    _cancellationTokenSource.Token);
                _output.WriteInfo($"Operation completed");
            }
        }
    }
}