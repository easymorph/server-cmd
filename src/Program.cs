using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using MorphCmd.Utils;
using MorphCmd.BusinessLogic;
using System.IO;
using MorphCmd.Models;
using Morph.Server.Sdk.Exceptions;

namespace MorphCmd
{
    class Program
    {

        static void Main(string[] args)
        {
            //TODO: add exit code for non-success execution

            var output = new ConsoleOutput();
            try
            {
                
                if (args.Length < 2)
                {
                    RunUsageSamples.WriteCreds(output);
                    Environment.Exit(1);
                }

                var paramsDict = CmdParametersHelper.ParseParams(args);
                var parameters = ParametersHelper.ExtractParameters(args[0], args[1], paramsDict);

                MainAsync(parameters).Wait();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File not found " + ex.FileName);
                Environment.Exit(1);

            }

            catch (AggregateException agr)
            {
                foreach (var e in agr.InnerExceptions)
                {
                    output.WriteError(e.Message);
                    if(e is ResponseParseException rpe)
                    {
                        output.WriteError(rpe.ServerResponseString);
                    }
                    Exception inner = e.InnerException;
                    if (inner != null)
                    {
                        while (inner.InnerException != null)
                        {
                            output.WriteError(inner.Message);
                            inner = inner.InnerException;
                        }
                    }
                }
                Environment.Exit(1);

            }
            catch (Exception ex)
            {
                output.WriteError(ex.Message);
                Exception inner = ex.InnerException;
                if (inner != null)
                {                    
                    while (inner.InnerException != null)
                    {
                        output.WriteError(inner.Message);
                        inner = inner.InnerException;
                    }
                }
                
                
                Environment.Exit(1);
            }
        }



        static async Task MainAsync(Parameters parameters)
        {

            var apiClient = new MorphServerApiClient(parameters.Host);
            var output = new ConsoleOutput();
            var input = new ConsoleInput();
            var handler = new CommandsHandler(output, input, apiClient);
            await handler.Handle(parameters);

        }
    }



}
