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
    internal class GetTaskCommand : BaseCommand, ICommand
    {
        public GetTaskCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (parameters.TaskId == null)
            {
                throw new WrongCommandFormatException("TaskId is required");
            }

            _output.WriteInfo($"Attempting to get task {parameters.TaskId.Value.ToString("D")} in space '{parameters.SpaceName}'");
            
            using (var apiSession = await OpenSession(parameters))
            {

                var task = await _apiClient.GetTaskAsync(apiSession, parameters.TaskId.Value, _cancellationTokenSource.Token);
                _output.WriteInfo("Info about task:");
                _output.WriteInfo(string.Format("Id:'{0}'", task.Id));
                _output.WriteInfo(string.Format("Name:'{0}'", task.Name));
                _output.WriteInfo(string.Format("IsRunning:'{0}'", task.IsRunning));                
                _output.WriteInfo(string.Format("Enabled:'{0}'", task.Enabled));
                _output.WriteInfo(string.Format("Note:'{0}'", task.Note));
                _output.WriteInfo(string.Format("ProjectPath:'{0}'", task.ProjectPath));
                _output.WriteInfo(string.Format("StatusText:'{0}'", task.StatusText));
                _output.WriteInfo(string.Format("TaskState:'{0}'", task.TaskState));
                _output.WriteInfo("Task Parameters:");
                foreach (var parameter in task.TaskParameters)
                {
                    _output.WriteInfo($"Parameter '{parameter.Name}' = '{parameter.Value}' [{parameter.ParameterType}] (Note: {parameter.Note})");
                }
                _output.WriteInfo("Done");

            }

        }
    }
}
