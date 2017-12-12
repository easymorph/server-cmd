using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Utils
{
    internal static class CmdParametersHelper
    {
        public static Dictionary<string, string> ParseParams(string[] args)
        {
            var result = new Dictionary<string, string>();

            Queue<string> paramsQ = new Queue<string>(args);
            try
            {
                while (paramsQ.Count > 0)
                {
                    var param = paramsQ.Dequeue();
                    if (param.StartsWith("-"))
                    {
                        var paramName = param.Substring(1).Trim().ToLowerInvariant();
                        if (paramsQ.Count == 0)
                            throw new Exception("Wrong formatted value for paramter " + paramName);

                        if (paramName.StartsWith("param:"))
                        {
                            paramName = "param:" + param.Substring(1 + "param:".Length).Trim();
                        }
                       
                        var paramValue = paramsQ.Dequeue();
                        result[paramName] = paramValue;
                    }
                    else if (param.StartsWith("/"))
                    {
                        var paramName = param.Substring(1).Trim().ToLowerInvariant();
                        result[paramName] = "true";
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to parse command parameters: " + e.Message);
            }          
        }
    }
}
