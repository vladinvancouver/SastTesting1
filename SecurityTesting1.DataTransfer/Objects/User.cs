using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityTesting1.DataTransfer.Objects
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = String.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
}
