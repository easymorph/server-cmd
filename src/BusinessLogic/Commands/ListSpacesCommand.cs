using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{

    internal class ListSpacesCommand : BaseCommand, ICommand
    {
        public ListSpacesCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => false;

        public async Task Execute(Parameters parameters)
        {


            var list = await _apiClient.GetSpacesListAsync(_cancellationTokenSource.Token);
            _output.WriteInfo("Available spaces:");

            foreach (var space in list.Items)
            {
                _output.WriteInfo(string.Format("{0} {1}", space.IsPublic ? " " : "*", space.SpaceName));
            }

            _output.WriteInfo("Listing done");
        }


    }
}
