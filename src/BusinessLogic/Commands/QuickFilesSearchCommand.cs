using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Model;
using MorphCmd.Interfaces;
using MorphCmd.Models;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic.Commands
{
    internal class QuickFilesSearchCommand : BaseCommand, ICommand
    {
        public QuickFilesSearchCommand(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient) : base(output, input, apiClient)
        {

        }

        public bool IsApiSessionRequired => true;

        public async Task Execute(Parameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Location))
            {
                _output.WriteInfo("Searching in the root folder of the space " + parameters.SpaceName);
            }
            else
            {
                _output.WriteInfo("Searching in the folder '" + parameters.Location + "' of the space " + parameters.SpaceName);
            }

            using (var apiSession = await OpenSession(parameters))
            {

                var request = new SpaceFilesQuickSearchRequest
                {
                    FolderPath = parameters.Location,
                    LookupString = parameters.LookupString,
                    FileExtensions = parameters.FileExtensions == null ?
                                      null :
                                        (parameters.FileExtensions).Split(';')
                };
                var data = await _apiClient.SpaceFilesQuickSearchAsync(
                    apiSession,
                    parameters.SpaceName,
                    request,
                    _cancellationTokenSource.Token,
                    parameters.Offset,
                    parameters.Limit
                    );


                foreach (var folder in data.Values)
                {
                    _output.WriteInfo(folder.Path);
                    _output.WriteInfo(string.Format("{0}{1} {2}", folder.LastModified.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt").PadRight(30), "<DIR>".PadRight(16), folder.Name));

                    foreach (var file in folder.Files)
                    {
                        _output.WriteInfo(string.Format("{0}{1} {2}", file.LastModified.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt").PadRight(30), file.FileSizeBytes.ToString("n0").PadLeft(16), file.Name));
                    }
                }

                _output.WriteInfo(data.HasMore ? "Has more data" : "Has no more data");

                _output.WriteInfo("Listing done");
            }

        }
    }
}
