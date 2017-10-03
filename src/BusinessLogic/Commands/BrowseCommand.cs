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

    internal class BrowseCommand : BaseCommand, ICommand
    {
        public BrowseCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Location))
            {
                _output.WriteInfo("Browsing the root folder of the space " + parameters.Space);
            }
            else
            {
                _output.WriteInfo("Browsing the folder '" + parameters.Location + "' of the space " + parameters.Space);
            }
            var data = await _apiClient.BrowseSpaceAsync(parameters.Space, parameters.Location, _cancellationTokenSource.Token);
            _output.WriteInfo("Space: " + data.SpaceName);
            _output.WriteInfo("Free space: " + data.FreeSpaceBytes + " bytes");
            foreach (var folder in data.Folders)
            {
                _output.WriteInfo(string.Format("{0}{1} {2}", folder.LastModified.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt").PadRight(30), "<DIR>".PadRight(16), folder.Name));
            }
            foreach (var file in data.Files)
            {
                _output.WriteInfo(string.Format("{0}{1} {2}", file.LastModified.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt").PadRight(30), file.FileSizeBytes.ToString("n0").PadLeft(16), file.Name));
            }


            _output.WriteInfo("Listing done");

        }
    }
}
