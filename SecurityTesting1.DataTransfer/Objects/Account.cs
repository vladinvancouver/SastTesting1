using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityTesting1.DataTransfer.Objects
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
    }
}
