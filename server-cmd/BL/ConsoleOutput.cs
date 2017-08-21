using MorphCmd.Interfaces;
using System;
using System.Text;

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
        public void WriteSymbols(string s)
        {
            Console.Write(s);

        }
        public void WriteError(string s)
        {
            Console.WriteLine("\nERROR:" + s);

        }

        public void Write(StringBuilder sb)
        {
            Console.Write(sb);
        }
    }
}
