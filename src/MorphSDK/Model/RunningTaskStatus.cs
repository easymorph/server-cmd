using System;

namespace MorphSDK.Model
{
    public class RunningTaskStatus
    {
        public Guid Id { get; set; }
        public bool IsRunning { get; set; }
        public string ProjectName { get; set; }
    }
}
