using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Exceptions
{
    internal class MissingParameterException : Exception
    {
        public MissingParameterException(string message) : base(message)
        {

        }
    }
    internal class WrongCommandFormatException : Exception
    {
        public WrongCommandFormatException(string message) : base(message)
        {

        }
    }

    internal class CommandFailedException : Exception
    {
        public CommandFailedException() : base("command failed")
        {

        }
    }
}
