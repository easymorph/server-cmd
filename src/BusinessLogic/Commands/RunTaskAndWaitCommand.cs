using Morph.Server.Sdk.Client;
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

        public async Task Execute(Parameters parameters)
        {
            if (parameters.TaskId == null)
            {
                throw new WrongCommandFormatException("TaskId is required");
            }
            _output.WriteInfo("Attempting to start task " + parameters.TaskId.Value.ToString("D"));
            var info = await _apiClient.StartTaskAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token);

            _output.WriteInfo(string.Format("Project '{0}' is running. Waiting until done.", info.ProjectName));
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            while ((await _apiClient.GetRunningTaskStatusAsync(parameters.Space, parameters.TaskId.Value, _cancellationTokenSource.Token)).IsRunning);
            //TODO: failed 
            _output.WriteInfo(string.Format("\nTask {0} completed", parameters.TaskId.Value.ToString("D")));

        }
    }
}
