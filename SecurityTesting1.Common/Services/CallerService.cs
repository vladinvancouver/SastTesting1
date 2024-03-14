using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityTesting1.Common.Services
{
    public class CallerService
    {
        public string PerformedBy { get; set; } = String.Empty;
        public string FromRemoteIpAddress { get; set; } = String.Empty;
        public string UserAgent { get; set; } = String.Empty;
    }
}
