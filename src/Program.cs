using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using MorphCmd.Utils;
using MorphCmd.BusinessLogic;
using System.IO;
using MorphCmd.Models;
using Morph.Server.Sdk.Exceptions;
using System.Security.Authentication;

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
                foreach (var e in agr.Flatten().InnerExceptions)
                {
                    TraverseExceptionTree(output, e);
                }
                Environment.Exit(1);

            }
            catch (Exception ex)
            {
                TraverseExceptionTree(output, ex);

                Environment.Exit(1);
            }
        }

        private static void TraverseExceptionTree(ConsoleOutput output, Exception e)
        {
            ProcessException(e, output);
            Exception inner = e.InnerException;            
            while (inner != null)
            {
                ProcessException(inner, output);
                inner = inner.InnerException;
            }
            
        }

        static void ProcessException(Exception e, ConsoleOutput consoleOutput)
        {
            consoleOutput.WriteError(e.Message);
            if (e is ResponseParseException rpe)
            {
                consoleOutput.WriteError(rpe.ServerResponseString);
            }
            if (e is System.Security.Authentication.AuthenticationException)
            {
                consoleOutput.WriteInfo("To prevent this error use a valid ssl certificate.");
                consoleOutput.WriteInfo("To suppress this error use /suppress-ssl-errors parameter.");
            }
        }


        static async Task MainAsync(Parameters parameters)
        {
            NetworkUtil.ConfigureServicePointManager(parameters.SuppressSslErrors);
            var apiClient = new MorphServerApiClient(parameters.Host);
            var output = new ConsoleOutput();
            var input = new ConsoleInput();
            var handler = new CommandsHandler(output, input, apiClient);
            await handler.Handle(parameters);

        }
    }



}
