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
using System.Net;
using System.Reflection;
#if NETCOREAPP2_0
using System.Net.Http;
#endif

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
            Exception exception = e;
            Exception previousException = null;
            while (exception != null)
            {                
                ProcessException(previousException, exception, output);
                previousException = exception;
                exception = exception.InnerException;
            }
            
        }

        static void ProcessException(Exception previousException, Exception e, ConsoleOutput consoleOutput)
        {
            
            if (e is ResponseParseException rpe)
            {
                consoleOutput.WriteError(e.Message);
                consoleOutput.WriteError(rpe.ServerResponseString);
            }

#if NETCOREAPP2_0
            else if (e is HttpRequestException m && m.HResult == -2147012721)
#elif NET45
            else if (e is AuthenticationException)
#endif
            {
                consoleOutput.WriteError(e.Message);
                consoleOutput.WriteInfo("To prevent this error use a valid ssl certificate.");
                consoleOutput.WriteInfo("To suppress this error use /suppress-ssl-errors parameter.");
            }

            else if(
                    previousException != null && 
                    previousException is WebException we &&
                    we.Status == WebExceptionStatus.SendFailure  &&
                    e is IOException)
            {
                consoleOutput.WriteError("Handshake failed. You're trying to connect over HTTPS, but server is configured for HTTP. Or vice versa.");
            }
            else
            {
                consoleOutput.WriteError(e.Message);
            }
            
        }


        static async Task MainAsync(Parameters parameters)
        {
            try
            {
                NetworkUtil.ConfigureServicePointManager(parameters.SuppressSslErrors);
#if NETCOREAPP2_0
                NetworkUtil.ConfigureServerCertificateCustomValidationCallback(parameters.SuppressSslErrors);
#endif
                //MorphServerApiClientGlobalConfig.FileTransferTimeout = TimeSpan.FromSeconds(2);

                
                Assembly thisAssem = typeof(Program).Assembly;
                var assemblyVersion = thisAssem.GetName().Version;
                MorphServerApiClientGlobalConfig.ClientId = "EasyMorph ems-cmd/"+ assemblyVersion.ToString();
                MorphServerApiClientGlobalConfig.ClientType = "ems-cmd/Native";

                var apiClient = new MorphServerApiClient(new Uri(parameters.Host));
                var output = new ConsoleOutput();
                var input = new ConsoleInput();
                var handler = new CommandsHandler(output, input, apiClient);
                await handler.Handle(parameters);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }



}
