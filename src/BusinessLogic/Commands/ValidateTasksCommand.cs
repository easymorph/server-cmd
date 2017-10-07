using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Exceptions;
using MorphCmd.Exceptions;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class ValidateTasksCommand : BaseCommand, ICommand
    {
        public ValidateTasksCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Location))
            {
                throw new WrongCommandFormatException("Location is required");
            }
            if (string.IsNullOrWhiteSpace(parameters.Space))
            {
                throw new WrongCommandFormatException("Space is required");
            }

            try
            {
                _output.WriteInfo("Validating tasks for the project '" + parameters.Location + "'");
                var result = await _apiClient.ValidateTasksAsync(parameters.Space, parameters.Location, _cancellationTokenSource.Token);

                if (result.FailedTasks.Count == 0)
                {
                    _output.WriteInfo("All tasks are valid");
                }
                else
                {
                    _output.WriteError(result.Message);
                    foreach (var item in result.FailedTasks)
                    {
                        _output.WriteInfo(item.TaskId + ": " + item.Message + "@" + item.TaskApiUrl);
                    }
                }
            }

            catch (MorphApiBadArgumentException ba)
            {
                _output.WriteError(ba.Message);
                foreach (var e in ba.Details)
                {
                    _output.WriteInfo(e.Field + ": " + e.Message);
                }
                throw new CommandFailedException();
            }

        }
    }

}
