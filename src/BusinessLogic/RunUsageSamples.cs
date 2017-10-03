﻿using MorphCmd.Interfaces;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic
{
    internal static class RunUsageSamples
    {

        public static void WriteCommadUsage(Command cmd, IOutputEndpoint output)
        {

            switch (cmd)
            {
                case Command.Run:
                    output.WriteInfo("RUN usage sample: ems-cmd run http://10.20.30.40:6330 -space Default -taskID 8de5b50a-2d65-44f4-9e86-660c2408fb06");
                    break;
                case Command.RunAsync:
                    output.WriteInfo("RUNASYNC usage sample: ems-cmd run http://10.20.30.40:6330 -space Default -taskID 8de5b50a-2d65-44f4-9e86-660c2408fb06");
                    break;
                case Command.Upload:
                    output.WriteInfo(@"UPLOAD usage sample: ems-cmd upload http://10.20.30.40:6330 -space Default  -source ""C:\Users\Public\Documents\Morphs\sample.morph"" -destination ""\folder 1""");
                    break;
                case Command.Download:
                    output.WriteInfo(@"DOWNLOAD usage sample: ems-cmd download http://10.20.30.40:6330 -space Default -source ""folder 1\file.map""  -destination ""C:\Users\Public\Documents\Morphs""");
                    break;
                case Command.Del:
                    output.WriteInfo("DEL usage sample: ems-cmd del http://10.20.30.40:6330 -space Default -location \"folder 1\\sample.txt\" ");
                    break;
                default:
                    output.WriteInfo("This command hasn't usage example");
                    break;


            }
        }

        internal static void WriteCreds(IOutputEndpoint output)
        {
            output.WriteInfo("EasyMorph Server command line client");
            output.WriteInfo("Usage sample: ems-cmd <command> <url> -param1 value -param2 value2");
            // ems-cmd upload http://10.20.30.40:6330 -space Default -from "C:\\Users\\Public\\Documents\\Morphs\\sample.morph" -to "folder 1" /y
            //  space and to are not required               
            output.WriteInfo("<command> - Supported commands: status, run, runasync, upload, download, del, browse ");
            output.WriteInfo("<url> - path to the server, e.g. http://10.20.30.40:6330 ");
        }
    }
}
