using MorphCmd.Interfaces;
using MorphCmd.Models;
using MorphCmd.Utils;
using Morph.Server.Sdk.Client;
using Morph.Server.Sdk.Events;
using Morph.Server.Sdk.Exceptions;
using Morph.Server.Sdk.Model;
using Morph.Server.Sdk.Model.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MorphCmd.BusinessLogic.Commands;
using MorphCmd.Exceptions;

namespace MorphCmd.BusinessLogic
{
    
    internal class CommandsHandler
    {
        private readonly IOutputEndpoint _output;
        private readonly IInputEndpoint _input;
        protected readonly IMorphServerApiClient _apiClient;
        private readonly CancellationTokenSource _cancellationTokenSource;


        public CommandsHandler(IOutputEndpoint output, IInputEndpoint input, IMorphServerApiClient apiClient)
        {
            _output = output;
            _input = input;
            _apiClient = apiClient;
            _cancellationTokenSource = new CancellationTokenSource();

        }
        public async Task Handle(Parameters parameters)
        {            

            var cmd = CommandsFactory.Construct(parameters.Command, _output, _input, _apiClient);
            try
            {
                await cmd.Execute(parameters);
            }
            catch (WrongCommandFormatException wrg)
            {
                _output.WriteError("Wrong command format");
                
                RunUsageSamples.WriteCommadUsage(parameters.Command, _output);

                throw;
            }

        }
    }
}
