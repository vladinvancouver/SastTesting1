using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityTesting1.Common.AuditEvents
{
    public class AccountDeletedAuditEvent001 : AuditEvent
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; } = String.Empty;
        public string PerformedBy { get; set; } = String.Empty;
        public string FromRemoteIpAddress { get; set; } = String.Empty;
        public string UserAgent { get; set; } = String.Empty;
        public DateTime OccurredUtcDate { get; set; }
        public string Message { get; set; } = String.Empty;
    }
}
