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
        public string Space { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Location { get; set; }
        
        public Guid? TaskId { get; set; }
        public bool YesToAll { get; set; }
        public string Host { get; set; }
    }
}
