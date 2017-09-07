using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Dto.Commands
{
    [DataContract]
    internal class ValidateTasksRequestDto
    {
        [DataMember(Name = "spaceName")]
        public string SpaceName { get; set; }
        [DataMember(Name = "projectPath")]
        public string ProjectPath { get; set; }
    }
}
