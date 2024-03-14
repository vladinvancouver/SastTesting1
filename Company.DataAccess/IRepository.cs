using System;
using System.Collections.Generic;
using System.Text;

namespace Company.DataAccess
{
    public interface IRepository
    {
        IUnitOfWork? UnitOfWork { get; set; }
    }
}
