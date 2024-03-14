using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Company.DataAccess
{
    public interface ICache<T>
    {
        Task<IEnumerable<T>> GetAsync();

        Task SetAsync(IEnumerable<T> data);

        Task ClearAsync();
    }
}
