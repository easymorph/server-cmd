using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Morph.Server.Sdk.Client;
using MorphCmd.Utils;
using MorphCmd.BusinessLogic;
using System.IO;
using MorphCmd.Models;

namespace MorphCmd
{
    class Program
    {

        static void Main(string[] args)
        {
            //TODO: add exit code for non-success execution

            var output = new ConsoleOutput();
            if (args.Length < 2)
            {
                Console.WriteLine("Not all parameters were specified");
                Console.WriteLine("Usage sample: ems-cmd <command> <url> -param1 value -param2 value2");
                // ems-cmd upload http://10.20.30.40:6330 -space Default -from "C:\\Users\\Public\\Documents\\Morphs\\sample.morph" -to "folder 1" /y
                //  space and to are not required               
                Console.WriteLine("<command> - Supported commands: status, run, runasync, upload, download ");
                Console.WriteLine("<url> - path to the server, e.g. http://10.20.30.40:6330 ");
                Environment.Exit(1);
            }
            try
            {
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
                }
                Environment.Exit(1);

            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException;
                if (inner != null)
                {
                    while (inner.InnerException != null)
                    {
                        inner = inner.InnerException;
                    }
                }
                output.WriteError(ex.Message);
                if (inner != null)
                {
                    output.WriteError(inner.Message);
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
