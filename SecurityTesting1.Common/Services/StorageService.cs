using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.DataAccess;

namespace SecurityTesting1.Common.Services
{
    //By default, closing a database connection does not actually close it but instead returns
    //it to a connection pool. Unfortunately, transaction isolation level settings are not reset when this
    //happens. Because we require a certain isolation level on some database connections, we need two
    //different connection strings so that there will be two connection pools.
    //https://github.com/dotnet/SqlClient/issues/96


    /// <summary>
    /// Use this service to access data storage.
    /// </summary>
    public class StorageService
    {
        private readonly ConcurrentDictionary<Type, object> _caches = new();

        public StorageService(IUnitOfWork forGeneralUseUnitOfWork, IUnitOfWork forUseWithTransactionsUnitOfWork)
        {
            ForGeneralUseUnitOfWork = forGeneralUseUnitOfWork ?? throw new ArgumentNullException(nameof(forGeneralUseUnitOfWork));
            ForUseWithTransactionsUnitOfWork = forUseWithTransactionsUnitOfWork ?? throw new ArgumentNullException(nameof(forUseWithTransactionsUnitOfWork));
        }

        public IUnitOfWork ForGeneralUseUnitOfWork { get; private set; }
        public IUnitOfWork ForUseWithTransactionsUnitOfWork { get; private set; }

        public void AddCacheDefinition<T>(Func<Task<IEnumerable<T>>> getDataActionAsync, TimeSpan expirationPeriod) where T : class
        {
            _caches.AddOrUpdate(typeof(T), new MemoryCache<T>(getDataActionAsync, expirationPeriod), (key, oldValue) => new MemoryCache<T>(getDataActionAsync, expirationPeriod));
        }

        public async Task<IEnumerable<T>> GetFromCacheAsync<T>() where T : class
        {
            if (_caches.TryGetValue(typeof(T), out object? value))
            {
                return await ((ICache<T>)value).GetAsync();
            }

            throw new Exception($"Cannot find cache for '{typeof(T)}'.");
        }

        public async Task ClearCacheAsync<T>() where T : class
        {
            if (_caches.TryGetValue(typeof(T), out object? value))
            {
                await ((ICache<T>)value).ClearAsync();
                return;
            }

            throw new Exception($"Cannot find cache for '{typeof(T)}'.");
        }
    }
}
