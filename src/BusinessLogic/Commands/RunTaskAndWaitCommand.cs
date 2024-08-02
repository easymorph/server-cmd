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
            _output.WriteInfo($"Attempting to start task {parameters.TaskId.Value.ToString("D")} in space '{parameters.SpaceName}'");
            foreach (var parameter in parameters.TaskRunParameters)
            {
                _output.WriteInfo($"Parameter '{parameter.Name}' = '{parameter.Value}'");
            }

            using (var apiSession = await OpenSession(parameters))
            {
                ComputationDetailedItem info = await _apiClient.StartTaskAsync(
                    apiSession,
                    parameters.SpaceName,
                    new StartTaskRequest(parameters.TaskId.Value)
                    {
                        TaskParameters = parameters.TaskRunParameters.Select(x => new ParameterNameValue(x.Name, x.Value)).ToArray()
                    },
                    _cancellationTokenSource.Token);


                _output.WriteInfo(string.Format("Project '{0}' is running. Waiting until done.", info.ProjectDetails.ProjectName));

                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                while ((info = await _apiClient.GetComputationDetailsAsync(apiSession, parameters.SpaceName, info.ComputationId, _cancellationTokenSource.Token)).State.IsRunning);

                if (info.State is ComputationState.Finished finished)
                {

                    var workflowResult = await _apiClient.GetWorkflowResultDetailsAsync(apiSession,
                        parameters.SpaceName,
                        finished.ResultObtainingToken, _cancellationTokenSource.Token);
                    try
                    {
                        switch (workflowResult.Result)
                        {
                            case WorkflowResultCode.Success:
                                _output.WriteInfo(string.Format("\nTask {0} completed",
                                    parameters.TaskId.Value.ToString("D")));
                                break;

                            case WorkflowResultCode.Failure:
                                _output.WriteInfo(string.Format("\nTask {0} failed",
                                    parameters.TaskId.Value.ToString("D")));
                                throw new CommandFailedException();
                                break;
                            case WorkflowResultCode.TimedOut:
                                _output.WriteInfo(string.Format("\nTask {0} Timed out",
                                    parameters.TaskId.Value.ToString("D")));
                                throw new CommandFailedException();
                                break;
                            case WorkflowResultCode.CanceledByUser:
                                _output.WriteInfo(string.Format("\nTask {0} canceled by user",
                                    parameters.TaskId.Value.ToString("D")));
                                throw new CommandFailedException();
                                break;
                            default:
                                _output.WriteInfo(string.Format("\nTask {0} finished with unknown state",
                                    parameters.TaskId.Value.ToString("D")));
                                throw new CommandFailedException();
                                break;

                        }
                    }
                    finally
                    {
                        await _apiClient.AcknowledgeWorkflowResultAsync(apiSession,
                            parameters.SpaceName,
                            info.ComputationId,
                            _cancellationTokenSource.Token);
                    }

                }
                
            }
        }
    }
}
