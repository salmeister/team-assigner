using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAssigner.Models
{
    public sealed class EmailSettings
    {
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public string FromEmail { get; set; }
        public string Psswd { get; set; }
    }
}
