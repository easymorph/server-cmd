using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Model
{
    
    public class ServerStatus
    {
    
        public string StatusCode { get; set; }
    
        public string StatusMessage { get; set; }
    
        public Version Version { get; set; }
    }

  

    
}
