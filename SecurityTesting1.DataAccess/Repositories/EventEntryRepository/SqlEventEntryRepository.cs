using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Company.DataAccess;
using Microsoft.Extensions.Logging;
using SecurityTesting1.DataAccess.Objects;
using SecurityTesting1.DataAccess.Databases;

namespace SecurityTesting1.DataAccess.Repositories.EventEntryRepository
{
    public class SqlEventEntryRepository : IEventEntryRepository
    {
        private readonly ILogger _logger;
        private ISqlUnitOfWork _unitOfWork = null!;

        public SqlEventEntryRepository(ILogger<SqlEventEntryRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task AddAsync(DataAccess.Objects.EventEntry eventEntry)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.AddEventEntry), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                EventEntryDatabase.AddEventEntry(_unitOfWork.ConnectionString, eventEntry);
            }
        }

        public async Task AddRangeAsync(IEnumerable<Objects.EventEntry> eventEntries)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.AddEventEntries), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                EventEntryDatabase.AddEventEntries(_unitOfWork.ConnectionString, eventEntries);
            }
        }

        public async Task AddRangeInBulkAsync(IEnumerable<Objects.EventEntry> eventEntries)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.AddEventEntriesInBulk), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds * 10))
            {
                EventEntryDatabase.AddEventEntriesInBulk(_unitOfWork.ConnectionString, eventEntries);
            }
        }

        public async Task<DataAccess.Objects.EventEntry?> GetNextSinceLastSequenceNumberAsync(IEnumerable<string> streamPaths, long lastSequenceNumber)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.GetNextEventEntrySinceLastSequenceNumber), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                return EventEntryDatabase.GetNextEventEntrySinceLastSequenceNumber(_unitOfWork.ConnectionString, streamPaths, lastSequenceNumber);
            }
        }

  
        public async Task<IEnumerable<EventEntry>> GetNextBatchSinceLastSequenceNumberAsync(IEnumerable<string> streamPaths, long lastSequenceNumber, int maxCount)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.GetNextEventEntryBatchSinceLastSequenceNumber), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                return EventEntryDatabase.GetNextEventEntryBatchSinceLastSequenceNumber(_unitOfWork.ConnectionString, streamPaths, lastSequenceNumber, maxCount);
            }
        }

  
        public async Task<long> GetLastSequenceNumberAsync(IEnumerable<string> streamPaths)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.GetLastSequenceNumber), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                return EventEntryDatabase.GetLastSequenceNumber(_unitOfWork.ConnectionString, streamPaths);
            }
        }

        public async Task DeleteAllInStreamAsync(string streamPath)
        {
            await Task.CompletedTask;

            using (Benchmark _ = new(_logger, message: nameof(EventEntryDatabase.DeleteEventEntriesInStream), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                EventEntryDatabase.DeleteEventEntriesInStream(_unitOfWork.ConnectionString, streamPath);
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
                _unitOfWork = (ISqlUnitOfWork)value;
            }
        }
    }
}
