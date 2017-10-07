using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic
{
    internal static class ParametersHelper
    {

        public static Parameters ExtractParameters(string command, string host, Dictionary<string, string> paramsDict)
        {
            Parameters parameters = new Parameters();
            parameters.Host = host;
            if (paramsDict.ContainsKey("space"))
                parameters.Space = paramsDict["space"];
            if (paramsDict.ContainsKey("source"))
                parameters.Source = paramsDict["source"];
            if (paramsDict.ContainsKey("destination"))
                parameters.Target = paramsDict["destination"];
            if (paramsDict.ContainsKey("target"))
                parameters.Target = paramsDict["target"];

            if (paramsDict.ContainsKey("location"))
                parameters.Location = paramsDict["location"];

            if (paramsDict.ContainsKey("y"))
                parameters.YesToAll = true;
            if (paramsDict.ContainsKey("taskid"))
            {
                Guid guid;
                if (!Guid.TryParse(paramsDict["taskid"], out guid))
                {
                    throw new Exception("The taskId is malformed. Expected: GUID like '8de5b50a-2d65-44f4-9e86-660c2408fb06'");

                }
                parameters.TaskId = guid;

            }

            if (Enum.TryParse<Command>(command.Trim(), true, out var cmd))
            {
                parameters.Command = cmd;
            }
            else
            {
                throw new Exception(string.Format("Command not supported: '{0}' ", command));
            }


            return parameters;
        }
    }

}
