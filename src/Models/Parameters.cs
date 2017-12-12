using Morph.Server.Sdk.Model;
using MorphCmd.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Models
{
    public class Parameters
    {
        public Command Command { get; set; }
        public string SpaceName { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Location { get; set; }
        
        public Guid? TaskId { get; set; }
        public bool YesToAll { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public List<TaskRunParameter> TaskRunParameters { get; set; }
        public Parameters()
        {
            TaskRunParameters = new List<TaskRunParameter>();
        }
    }

    public class TaskRunParameter
    {
        public string Name{ get; set; }
        public string Value { get; set; }

        public TaskRunParameter(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            this.Name = name;
            this.Value = value;
        }
    }
}
