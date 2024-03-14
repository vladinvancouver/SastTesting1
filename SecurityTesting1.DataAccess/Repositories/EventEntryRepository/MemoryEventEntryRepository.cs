using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Company.DataAccess;
using Company.Utilities;
using SecurityTesting1.DataAccess.Objects;

namespace SecurityTesting1.DataAccess.Repositories.EventEntryRepository
{
    public class MemoryEventEntryRepository : IEventEntryRepository
    {
        //To mimic SQL behaviour and force use of add or update methods, objects returned or added should be clones (different reference)
        private readonly List<DataAccess.Objects.EventEntry> _data = new();
        private IMemoryUnitOfWork _unitOfWork = null!;
        private int _nextId = 1;
        private object _locker = new object();

        public async Task AddAsync(EventEntry eventEntry)
        {
            await Task.CompletedTask;
            lock (_locker)
            {
                eventEntry.EventEntryId = _nextId++;
                eventEntry.PersistedUtcDate = SystemTime.UtcNow;
                _data.Add(eventEntry.Clone());
            }
        }

        public async Task AddRangeAsync(IEnumerable<Objects.EventEntry> eventEntries)
        {
            foreach (Objects.EventEntry eventEntry in eventEntries)
            {
                await AddAsync(eventEntry);
            }
        }

        public async Task AddRangeInBulkAsync(IEnumerable<Objects.EventEntry> eventEntries)
        {
            await AddRangeAsync(eventEntries.ToArray());
        }

        public async Task DeleteAllInStreamAsync(string streamPath)
        {
            await Task.CompletedTask;
            lock (_locker)
            {
                _data.RemoveAll(obj => (obj.StreamPath + "/").StartsWith(streamPath + "/"));
            }
        }

        public async Task<EventEntry?> GetNextSinceLastSequenceNumberAsync(IEnumerable<string> streamPaths, long lastSequenceNumber)
        {
            return (await GetNextBatchSinceLastSequenceNumberAsync(streamPaths, lastSequenceNumber, maxCount: 1)).FirstOrDefault();
        }

        public async Task<IEnumerable<EventEntry>> GetNextBatchSinceLastSequenceNumberAsync(IEnumerable<string> streamPaths, long lastSequenceNumber, int maxCount)
        {
            await Task.CompletedTask;
            lock (_locker)
            {
                List<EventEntry> results = new();
                foreach (EventEntry eventEntry in _data.Where(obj => obj.EventEntryId > lastSequenceNumber).OrderBy(obj => obj.EventEntryId))
                {
                    foreach (string streamPath in streamPaths)
                    {
                        if ((eventEntry.StreamPath + "/").StartsWith(streamPath + "/"))
                        {
                            results.Add(eventEntry);
                        }
                    }
                }
                return results.Take(maxCount);
            }
        }

        public async Task<long> GetLastSequenceNumberAsync(IEnumerable<string> streamPaths)
        {
            await Task.CompletedTask;
            lock (_locker)
            {
                List<EventEntry> results = new();
                foreach (EventEntry eventEntry in _data.OrderBy(obj => obj.EventEntryId))
                {
                    foreach (string streamPath in streamPaths)
                    {
                        if ((eventEntry.StreamPath + "/").StartsWith(streamPath + "/"))
                        {
                            results.Add(eventEntry);
                        }
                    }
                }

                if (results.Any())
                {
                    return results.Last().EventEntryId;
                }
                else
                {
                    return 0;
                }
            }
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork;
            }
            set
            {
                _unitOfWork = (IMemoryUnitOfWork)value;
            }
        }
    }
}
