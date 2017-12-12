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
    internal class RunTaskAndWaitCommand : BaseCommand, ICommand
    {
        

        public RunTaskAndWaitCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {
            
        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {            

            if (parameters.TaskId == null)
            {
                throw new WrongCommandFormatException("TaskId is required");
            }
            _output.WriteInfo($"Attempting to start task {parameters.TaskId.Value.ToString("D")} in space '{parameters.SpaceName}'" );
            foreach (var parameter in parameters.TaskRunParameters)
            {
                _output.WriteInfo($"Parameter '{parameter.Name}' = '{parameter.Value}'");
            }

            using (var apiSession = await OpenSession(parameters))
            {               

                var status = await _apiClient.GetTaskStatusAsync(apiSession, parameters.TaskId.Value, _cancellationTokenSource.Token);
                if (status.IsRunning)
                {
                    throw new Exception($"Task {parameters.TaskId.Value.ToString("D")} is already running. Exiting");
                }

                var info = await _apiClient.StartTaskAsync(
                    apiSession,
                    parameters.TaskId.Value,
                    _cancellationTokenSource.Token,
                    parameters.TaskRunParameters.Select(x => new TaskStringParameter(x.Name, x.Value)).ToArray());

                _output.WriteInfo(string.Format("Project '{0}' is running. Waiting until done.", info.ProjectName));

                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                while ((status = await _apiClient.GetTaskStatusAsync(apiSession, parameters.TaskId.Value, _cancellationTokenSource.Token)).IsRunning);
                if (status.TaskState != TaskState.Failed)
                {
                    _output.WriteInfo(string.Format("\nTask {0} completed", parameters.TaskId.Value.ToString("D")));
                }
                else
                {
                    _output.WriteInfo(string.Format("\nTask {0} failed", parameters.TaskId.Value.ToString("D")));
                }
            }
        }
    }
}
