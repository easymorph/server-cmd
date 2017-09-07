using MorphSDK.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Dto.Commands
{
    [DataContract(Name = "ValidateTasksError", Namespace = "")]
    internal class ValidateTasksErrorDto : InnerError
    {
        [DataMember(Name = "failedTasks")]
        public List<FailedTaskInfoDto> FailedTasks { get; set; }

        public ValidateTasksErrorDto()
        {
            FailedTasks = new List<FailedTaskInfoDto>();            
        }
    }
    [DataContract]
    public class FailedTaskInfoDto
    {
        [DataMember(Name = "taskId")]
        public string TaskId { get; set; }
        [DataMember(Name = "missingParameters")]
        public List<string> MissingParameters { get; set; }
        [DataMember(Name = "taskSiteUrl")]
        public string TaskSiteUrl { get; set; }
        [DataMember(Name = "taskLocation")]
        public string TaskLocation { get; set; }
        [DataMember(Name = "text")]
        public string Text { get; set; }

        public FailedTaskInfoDto()
        {
            MissingParameters = new List<string>();
        }
    }
}
