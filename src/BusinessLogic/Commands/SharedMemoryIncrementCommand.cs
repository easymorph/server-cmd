using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model.SharedMemory;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class SharedMemoryIncrementCommand : BaseCommand, ICommand
    {
        public SharedMemoryIncrementCommand(
            IOutputEndpoint output,
            IInputEndpoint input,
            IMorphServerApiClient apiClient)
            : base(output, input, apiClient)
        {
        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Key))
                throw new WrongCommandFormatException("Key is required");

            using (var session = await OpenSession(parameters))
            {
                var incrementBy = parameters.By ?? 1.0m;
                _output.WriteInfo(
                    $"Increment the shared memory record {parameters.Key} " +
                    $"in the space {parameters.SpaceName} " +
                    $"by {incrementBy}...");

                var incrementedValue = await _apiClient.SharedMemoryIncrement(
                    session, parameters.SpaceName, parameters.Key, incrementBy,
                    MissingKeyBehavior.Throw, _cancellationTokenSource.Token);

                if (incrementedValue is SharedMemoryValue.Number number)
                {
                    _output.WriteInfo($"Operation completed, new value is {number.Value}");
                }
            }
        }
    }
}
