using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Dto
{
    [DataContract]
    public class RunningTaskStatusDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "isRunning")]
        public bool IsRunning { get; set; }
        [DataMember(Name = "projectName")]
        public string ProjectName { get; set; }
    }
}
