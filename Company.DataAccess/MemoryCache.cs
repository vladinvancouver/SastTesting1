using Company.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Company.DataAccess
{
    /// <summary>
    /// A container to store cached data access objects from frequently run queries. Updates to this container replace existing data.
    /// There are two ways to get data into this container. First, you can use the push method to populate the data by calling the SetAsync() method.
    /// Alternatively you can use the pull method by specifying instructions on when and how to populate this container in the constructor
    /// which will be triggered by calling the GetAsync() method if the data is stale. The push method can be set by a background service.
    /// Use both methods so that the same code will work when run from unit tests where there is no background service to populate the container.
    /// </summary>
    public class MemoryCache<T> : ICache<T>
    {
        //Got to lock data otherwise another thread could get incorrect data while cache is updating (old data removed and new data added en masse).

        private List<T> _data = new List<T>();
        private object _locker = new object();
        private Func<Task<IEnumerable<T>>> _getDataActionAsync;
        private TimeSpan _expirationPeriod;
        private DateTime _lastUpdatedUtcDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

        public MemoryCache(Func<Task<IEnumerable<T>>> getDataActionAsync, TimeSpan expirationPeriod)
        {
            _getDataActionAsync = getDataActionAsync;
            _expirationPeriod = expirationPeriod;
        }

        public async Task<IEnumerable<T>> GetAsync()
        {
            if (SystemTime.UtcNow.Subtract(_lastUpdatedUtcDate) > _expirationPeriod)
            {
                IEnumerable<T> data = await _getDataActionAsync();
                await SetAsync(data);
            }

            lock(_locker)
            {
                return _data.ToArray();
            }
        }

        public async Task SetAsync(IEnumerable<T> data)
        {
            await Task.CompletedTask;
            lock(_locker)
            {
                _lastUpdatedUtcDate = SystemTime.UtcNow;
                _data.Clear();
                _data.AddRange(data);
            }
        }

        public async Task ClearAsync()
        {
            await Task.CompletedTask;
            lock (_locker)
            {
                _lastUpdatedUtcDate = DateTime.MinValue;
                _data.Clear();
            }
        }
    }
}
