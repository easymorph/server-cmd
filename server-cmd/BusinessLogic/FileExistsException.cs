using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic
{
    internal class FileExistsException:Exception
    {
        public FileExistsException(string message):base(message)
        {

        }
    }
}
