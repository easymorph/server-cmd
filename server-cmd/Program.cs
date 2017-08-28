using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MorphSDK.Client;
using MorphCmd.Utils;
using MorphCmd.BusinessLogic;

namespace MorphCmd
{
    class Program
    {      

        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("Not all parameters were specified");
                Console.WriteLine("Usage sample: ems-cmd <command> <url> -param1 value -param2 value2");
                // ems-cmd upload http://10.20.30.40:6330 -space Default -from "C:\\Users\\Public\\Documents\\Morphs\\sample.morph" -to "folder 1" /y
                //  space and to are not required               
                Console.WriteLine("<command> - Supported commands: status, run, runasync, upload, download ");
                Console.WriteLine("<url> - path to the server, e.g. http://10.20.30.40:6330 ");
                Environment.Exit(0);
            }
            var param = CmdParametersHelper.ParseParams(args);
            if(param == null)
            {
                Console.WriteLine("Unable to parse command parameters");
                Environment.Exit(-1);
            }
            MainAsync(args[0], args[1], param).Wait();
        }



        static async Task MainAsync(string command, string url, Dictionary<string, string> paramsDict)
        {
            var apiClient = new MorphServerApiClient(url);
            var output = new ConsoleOutput();
            var input = new ConsoleInput();
            var handler = new CommandsHandler(output, input, apiClient);
            await handler.Handle(command, paramsDict);
        }
    }



}
