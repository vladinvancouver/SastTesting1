using System;
using System.Collections.Generic;
using System.Text;

namespace Company.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        TRepositoryInterface GetRepository<TRepositoryInterface>() where TRepositoryInterface : IRepository;
        string Label { get; set; }
    }
}