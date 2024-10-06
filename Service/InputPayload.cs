using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal class InputPayload
    {
        public string MessageId { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
