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
            foreach (string arg in args.Where(x => x.StartsWith("-") || x.StartsWith("/")))
            {
                string[] parts = arg.Split('=');
                if (parts.Length > 0)
                {
                    string key = parts[0].Trim().ToLowerInvariant();
                    string val = parts[1].Trim();
                    if (key.Length > 1)
                    {
                        result[key.Substring(1)] = val;
                    }
                }
            }
            return result;
        }
    }
}
