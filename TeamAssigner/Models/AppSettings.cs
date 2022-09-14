using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAssigner.Models
{
    public sealed class AppSettings
    {
        public string Token { get; set; }
        public string ADOBaseURL { get; set; }
        public string VariableGroupIDToModify { get; set; }
        public int WeekOffset { get; set; }
    }
}
