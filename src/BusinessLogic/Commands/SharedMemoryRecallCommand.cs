using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model.SharedMemory;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class SharedMemoryRecallCommand : BaseCommand, ICommand
    {
        public SharedMemoryRecallCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(
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
                _output.WriteInfo($"Retrieving shared memory record {parameters.Key} in space {parameters.SpaceName}...");
                var result = await _apiClient.SharedMemoryRecall(session, parameters.SpaceName, parameters.Key, _cancellationTokenSource.Token);
                _output.WriteInfo(result.Contents.ToString());
            }
        }
    }
}