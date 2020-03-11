using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model;
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
    internal class RunTaskAndForgetCommand : BaseCommand, ICommand
    {
        public RunTaskAndForgetCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (parameters.TaskId == null)
            {
                throw new WrongCommandFormatException("TaskId is required");
            }

            _output.WriteInfo($"Attempting to start task {parameters.TaskId.Value.ToString("D")} in space '{parameters.SpaceName}'");
            foreach (var parameter in parameters.TaskRunParameters)
            {
                _output.WriteInfo($"Parameter '{parameter.Name}' = '{parameter.Value}'");
            }

            using (var apiSession = await OpenSession(parameters))
            {

                var info = await _apiClient.StartTaskAsync(apiSession,
                    new StartTaskRequest()
                    {
                        TaskId = parameters.TaskId.Value,
                        TaskParameters = parameters.TaskRunParameters.Select(x => new TaskStringParameter(x.Name, x.Value)).ToArray()
                    },
                    _cancellationTokenSource.Token
                    );
                _output.WriteInfo(string.Format("Project '{0}' is running.", info.ProjectName));
            }

        }
    }
}
