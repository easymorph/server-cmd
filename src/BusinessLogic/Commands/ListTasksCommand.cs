﻿using Morph.Server.Sdk.Client;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class ListTasksCommand : BaseCommand, ICommand
    {
        public ListTasksCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {        
            using (var apiSession = await OpenSession(parameters))
            {
                _output.WriteInfo(string.Format("Listing tasks in the space {0}", apiSession.SpaceName));
                var data = await _apiClient.GetTasksListAsync(apiSession,  _cancellationTokenSource.Token);
                
                foreach (var task in data.Items)
                {
                    _output.WriteInfo(string.Format("{0}: {1}", task.Id.ToString(), task.Name));
                }

                _output.WriteInfo("Listing done");
            }

        }
    }
}
