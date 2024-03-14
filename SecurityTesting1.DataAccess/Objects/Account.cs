using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityTesting1.DataAccess.Objects
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public bool IsMarkedForDeletion { get; set; }
        public string CreatedFromRemoteIpAddress { get; set; } = String.Empty;
        public string CreatedBy { get; set; } = String.Empty;
        public DateTime CreatedUtcDate { get; set; }
        public string CreatedUserAgent { get; set; } = String.Empty;
        public string UpdatedFromRemoteIpAddress { get; set; } = String.Empty;
        public string UpdatedBy { get; set; } = String.Empty;
        public DateTime UpdatedUtcDate { get; set; }
        public string UpdatedUserAgent { get; set; } = String.Empty;
    }
}
