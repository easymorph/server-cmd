using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Models
{
    public class Parameters
    {
        public string Command { get; set; }
        public string Space { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Location { get; set; }
        public string File { get; set; }
        public Guid? TaskId { get; set; }
        public bool YesToAll { get; set; }
    }
}
