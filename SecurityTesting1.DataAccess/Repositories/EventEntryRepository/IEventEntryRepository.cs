using Company.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SecurityTesting1.DataAccess.Repositories.EventEntryRepository
{
    public interface IEventEntryRepository : IRepository
    {
        Task AddAsync(DataAccess.Objects.EventEntry eventEntry);

        Task AddRangeAsync(IEnumerable<Objects.EventEntry> eventEntries);

        Task AddRangeInBulkAsync(IEnumerable<Objects.EventEntry> eventEntries);

        Task<DataAccess.Objects.EventEntry?> GetNextSinceLastSequenceNumberAsync(IEnumerable<string> streamPaths, long lastSequenceNumber);

        Task<IEnumerable<DataAccess.Objects.EventEntry>> GetNextBatchSinceLastSequenceNumberAsync(IEnumerable<string> streamPaths, long lastSequenceNumber, int maxCount);

        Task<long> GetLastSequenceNumberAsync(IEnumerable<string> streamPaths);

        Task DeleteAllInStreamAsync(string streamPath);
    }
}
