using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Models
{
    public enum Command
    {
        Run,
        RunAsync,
        GetTask,
        Status,
        Upload,
        Browse,
        FilesSearch,
        Download,
        Del,
        ValidateTasks,
        ListSpaces,
        SpaceStatus,
        ListTasks,
        HttpSecureChallengeTest,
        
        // Shared memory commands
        Remember,
        Recall,
        Forget,
    }
}
