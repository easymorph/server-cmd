using Morph.Server.Sdk.Client;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal static class CommandsFactory
    {
        public static ICommand Construct(Command command, IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient)
        {
            switch (command)
            {
                case Command.Run:
                    return new RunTaskAndWaitCommand(output, input, apiClient);
                case Command.RunAsync:
                    return new RunTaskAndForgetCommand(output, input, apiClient);
                case Command.GetTask:
                    return new GetTaskCommand(output, input, apiClient);
                case Command.Status:
                    return new ServerStatusCommand(output, input, apiClient);
                case Command.Upload:
                    return new UploadFileCommand(output, input, apiClient);
                case Command.Browse:
                    return new BrowseFilesCommand(output, input, apiClient);
                case Command.Del:
                    return new DeleteFileCommand(output, input, apiClient);
                case Command.Download:
                    return new DownloadFileCommand(output, input, apiClient);
                case Command.ValidateTasks:
                    return new ValidateTasksCommand(output, input, apiClient);
                case Command.ListSpaces:
                    return new ListSpacesCommand(output, input, apiClient);
                case Command.SpaceStatus:
                    return new SpaceStatusCommand(output, input, apiClient);
                case Command.ListTasks:
                    return new ListTasksCommand(output, input, apiClient);
                case Command.HttpSecureChallengeTest:
                    return new HttpSecureChallengeTestCommand(output, input, apiClient);
                default:
                    throw new Exception("Command not supported");
            }
        }
    }
}
