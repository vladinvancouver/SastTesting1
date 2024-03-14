using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Company.DataAccess
{
    public interface ISqlUnitOfWork : IUnitOfWork
    {
        public string ConnectionString { get; }
        public int WarningThresholdInMilliseconds { get; }
    }
}
