using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Interfaces
{
    internal interface IOutputEndpoint
    {
        void WriteInfo(string s);
        void WriteError(string s);
        bool IsOutputRedirected { get; }
    }
}
