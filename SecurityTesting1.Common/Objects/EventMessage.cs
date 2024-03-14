using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Company.Utilities;

namespace SecurityTesting1.Common.Objects
{
    public class EventMessage
    {
        public EventMessage(string streamPath, object @event)
        {
            StreamPath = streamPath;
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            OccurredUtcDate = SystemTime.UtcNow;
            NoticedUtcDate = OccurredUtcDate;
        }

        public string StreamPath { get; set; } = String.Empty;
        public object Event { get; set; }
        public DateTime OccurredUtcDate { get; set; }
        public DateTime NoticedUtcDate { get; set; }
        public string PerformedBy { get; set; } = String.Empty;
        public string FromRemoteIpAddress { get; set; } = String.Empty;
        public string UserAgent { get; set; } = String.Empty;

    }
}
