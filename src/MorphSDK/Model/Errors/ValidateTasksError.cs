using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Model.Errors
{

    public class ValidateTasksError
    {
        /// <summary>
        /// List of failed tasks
        /// </summary>
        public List<FailedTaskInfo> FailedTasks { get; set; }

        public ValidateTasksError()
        {
            FailedTasks = new List<FailedTaskInfo>();
        }
    }

    public class FailedTaskInfo
    {
       
        public string TaskId { get; set; }
       
        public List<string> MissingParameters { get; set; }

        public string TaskSiteUrl { get; set; }

        public string TaskLocation { get; set; }

        public string Text { get; set; }

        public FailedTaskInfo()
        {
            MissingParameters = new List<string>();
        }
    }

}
