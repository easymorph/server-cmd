using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Dto
{

    [DataContract]
    internal class ErrorResponse
    {
        [DataMember]
        public Error error { get; set; }
    }
   
}
