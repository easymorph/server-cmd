using MorphCmd.Interfaces;
using System;

namespace MorphCmd.BL
{
    internal class ConsoleOutput : IOutputEndpoint
    {
        public bool IsOutputRedirected
        {
            get
            {
                return Console.IsOutputRedirected;
            }
        }

        public void WriteInfo(string s)
        {
            Console.WriteLine(s);

        }
        public void WriteError(string s)
        {
            Console.WriteLine("ERROR:" + s);

        }

    }
}
