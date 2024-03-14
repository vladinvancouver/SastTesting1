using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Company.Utilities;
using ProtoBuf;
using SecurityTesting1.Common.Helpers;
using SecurityTesting1.Common.Objects;
using SecurityTesting1.Common.Services;
using SecurityTesting1.DataAccess.Objects;
using SecurityTesting1.DataAccess.Repositories.EventEntryRepository;

namespace SecurityTesting1.Common.Rules
{
    public static class EventEntryRules
    {
        public static async Task<long> GetLastSequenceNumberAsync(StorageService storageService, IEnumerable<string> streamPaths)
        {
            IEventEntryRepository eventEntryRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IEventEntryRepository>();
            return await eventEntryRepository.GetLastSequenceNumberAsync(streamPaths);
        }

        public static T GetEvent<T>(EventEntry eventEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            byte[] data;

            //Decompress
            if (eventEntry.CompressionType == CompressionType.None)
            {
                data = eventEntry.Data;
            }
            else if (eventEntry.CompressionType == CompressionType.GZip)
            {
                data = CommonRules.GZipDecompress(eventEntry.Data);
            }
            else
            {
                throw new NotSupportedException($"Compression type '{eventEntry.CompressionType}' is not supported.");
            }

            //Deserialize
            T? obj;
            if (eventEntry.SerializationType == SerializationType.ProtocolBuffers)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    obj = Serializer.Deserialize<T>(ms);
                }
            }
            else if (eventEntry.SerializationType == SerializationType.JSON)
            {
                //We expect JSON to be UTF-8 encoded.
                obj = System.Text.Json.JsonSerializer.Deserialize<T>(data.AsSpan(), jsonSerializerOptions);
            }
            else
            {
                throw new NotSupportedException($"Serialization type '{eventEntry.SerializationType}' is not supported for event entry ID '{eventEntry.EventEntryId}'.");
            }

            if (obj is null)
            {
                throw new Exception($"Cannot deserialize data to type '{eventEntry.DataType}' for event entry ID '{eventEntry.EventEntryId}'.");
            }

            return obj;
        }

        public static object GetEvent(EventEntry eventEntry, Type type, JsonSerializerOptions jsonSerializerOptions)
        {
            byte[] data;

            //Decompress
            if (eventEntry.CompressionType == CompressionType.None)
            {
                data = eventEntry.Data;
            }
            else if (eventEntry.CompressionType == CompressionType.GZip)
            {
                data = CommonRules.GZipDecompress(eventEntry.Data);
            }
            else
            {
                throw new NotSupportedException($"Compression type '{eventEntry.CompressionType}' is not supported.");
            }

            //Deserialize
            object? obj = null;
            if (eventEntry.SerializationType == SerializationType.ProtocolBuffers)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    obj = Serializer.Deserialize(type, ms);
                }
            }
            else if (eventEntry.SerializationType == SerializationType.JSON)
            {
                //We expect JSON to be UTF-8 encoded.
                obj = System.Text.Json.JsonSerializer.Deserialize(data.AsSpan(), type, jsonSerializerOptions);
            }
            else
            {
                throw new NotSupportedException($"Serialization type '{eventEntry.SerializationType}' is not supported for event entry ID '{eventEntry.EventEntryId}'.");
            }

            if (obj is null)
            {
                throw new Exception($"Cannot deserialize data to type '{eventEntry.DataType}' for event entry ID '{eventEntry.EventEntryId}'.");
            }

            return obj;
        }

        public static object GetEvent(EventEntry eventEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            byte[] data;

            //Decompress
            if (eventEntry.CompressionType == CompressionType.None)
            {
                data = eventEntry.Data;
            }
            else if (eventEntry.CompressionType == CompressionType.GZip)
            {
                data = CommonRules.GZipDecompress(eventEntry.Data);
            }
            else
            {
                throw new NotSupportedException($"Compression type '{eventEntry.CompressionType}' is not supported.");
            }

            //TODO: use better way to find types
            Type? type = Type.GetType(eventEntry.DataType);
            if (type is null)
            {
                throw new Exception($"Cannot find type given name '{eventEntry.DataType}' for event entry ID '{eventEntry.EventEntryId}'.");
            }

            //Deserialize
            object? obj = null;
            if (eventEntry.SerializationType == SerializationType.ProtocolBuffers)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    obj = Serializer.Deserialize(type, ms);
                }
            }
            else if (eventEntry.SerializationType == SerializationType.JSON)
            {
                //We expect JSON to be UTF-8 encoded.
                obj = System.Text.Json.JsonSerializer.Deserialize(data.AsSpan(), type, jsonSerializerOptions);
            }
            else
            {
                throw new NotSupportedException($"Serialization type '{eventEntry.SerializationType}' is not supported for event entry ID '{eventEntry.EventEntryId}'.");
            }

            if (obj is null)
            {
                throw new Exception($"Cannot deserialize data to type '{eventEntry.DataType}' for event entry ID '{eventEntry.EventEntryId}'.");
            }

            return obj;
        }

        public static async Task<long> SaveEventEntryAsync(StorageService storageService, JsonSerializerOptions jsonSerializerOptions, EventMessage eventMessage)
        {
            return await SaveEventEntryAsync(storageService, jsonSerializerOptions, eventMessage.Event, eventMessage.StreamPath, eventMessage.OccurredUtcDate, eventMessage.NoticedUtcDate, eventMessage.PerformedBy, eventMessage.FromRemoteIpAddress, eventMessage.UserAgent);
        }

        public static async Task<long> SaveEventEntryAsync(StorageService storageService, JsonSerializerOptions jsonSerializerOptions, object @event, string streamPath, DateTime occurredUtcDate, DateTime noticedUtcDate, string performedBy, string fromRemoteIpAddress, string userAgent)
        {
            byte[] data = new byte[] { };
            SerializationType serializationType;
            CompressionType compressionType = CompressionType.GZip;

            //Serialize
            if (ProtocolBuffersHelper.CanSerializeAsProtocolBuffers(@event.GetType()))
            {
                serializationType = SerializationType.ProtocolBuffers;
                data = ProtocolBuffersHelper.SerializeViaReflection(@event);
            }
            else
            {
                data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(@event, @event.GetType(), jsonSerializerOptions));
                serializationType = SerializationType.JSON;
            }

            //Compress
            if (compressionType == CompressionType.None)
            {
                //No action required
            }
            else if (compressionType == CompressionType.GZip)
            {
                data = CommonRules.GZipCompress(data);
            }
            else
            {
                throw new NotSupportedException($"Compression type '{compressionType}' is not supported.");
            }

            string objectType = @event.GetType().FullName ?? String.Empty;

            DateTime utcNow = SystemTime.UtcNow;

            DataAccess.Objects.EventEntry eventEntry = new()
            {
                StreamPath = streamPath,
                Data = data,
                DataType = objectType,
                CompressionType = compressionType,
                SerializationType = serializationType,
                OccurredUtcDate = occurredUtcDate,
                NoticedUtcDate = noticedUtcDate,
                ReceivedUtcDate = utcNow,
                CreatedBy = performedBy,
                CreatedFromRemoteIpAddress = fromRemoteIpAddress,
                CreatedUserAgent = userAgent
            };

            bool isInsideTransactionScope = System.Transactions.Transaction.Current != null;
            if (isInsideTransactionScope)
            {
                IEventEntryRepository txEventEntryRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IEventEntryRepository>();
                await txEventEntryRepository.AddAsync(eventEntry);
            }
            else
            {
                IEventEntryRepository eventEntryRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IEventEntryRepository>();
                await eventEntryRepository.AddAsync(eventEntry);
            }

            return eventEntry.EventEntryId;
        }

        public static async Task SaveEventEntriesAsync(StorageService storageService, JsonSerializerOptions jsonSerializerOptions, IEnumerable<EventMessage> eventMessages)
        {
            List<DataAccess.Objects.EventEntry> eventEntries = new();

            foreach (EventMessage eventMessage in eventMessages)
            {
                byte[] data = new byte[] { };
                SerializationType serializationType;
                CompressionType compressionType = CompressionType.GZip;

                //Serialize
                if (ProtocolBuffersHelper.CanSerializeAsProtocolBuffers(eventMessage.Event.GetType()))
                {
                    serializationType = SerializationType.ProtocolBuffers;
                    data = ProtocolBuffersHelper.SerializeViaReflection(eventMessage.Event);
                }
                else
                {
                    data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(eventMessage.Event, eventMessage.Event.GetType(), jsonSerializerOptions));
                    serializationType = SerializationType.JSON;
                }

                //Compress
                if (compressionType == CompressionType.None)
                {
                    //No action required
                }
                else if (compressionType == CompressionType.GZip)
                {
                    data = CommonRules.GZipCompress(data);
                }
                else
                {
                    throw new NotSupportedException($"Compression type '{compressionType}' is not supported.");
                }

                string objectType = eventMessage.Event.GetType().FullName ?? String.Empty;

                DateTime utcNow = SystemTime.UtcNow;

                DataAccess.Objects.EventEntry eventEntry = new()
                {
                    StreamPath = eventMessage.StreamPath,
                    Data = data,
                    DataType = objectType,
                    CompressionType = compressionType,
                    SerializationType = serializationType,
                    OccurredUtcDate = eventMessage.OccurredUtcDate,
                    NoticedUtcDate = eventMessage.NoticedUtcDate,
                    ReceivedUtcDate = utcNow,
                    CreatedBy = eventMessage.PerformedBy,
                    CreatedFromRemoteIpAddress = eventMessage.FromRemoteIpAddress,
                    CreatedUserAgent = eventMessage.UserAgent
                };

                eventEntries.Add(eventEntry);
            }

            bool isInsideTransactionScope = System.Transactions.Transaction.Current != null;
            if (isInsideTransactionScope)
            {
                IEventEntryRepository txEventEntryRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IEventEntryRepository>();
                await txEventEntryRepository.AddRangeInBulkAsync(eventEntries);
            }
            else
            {
                IEventEntryRepository eventEntryRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IEventEntryRepository>();
                await eventEntryRepository.AddRangeInBulkAsync(eventEntries);
            }
        }

        public static async Task DeleteEventsInStreamAsync(StorageService storageService, string streamPath)
        {
            IEventEntryRepository eventEntryRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IEventEntryRepository>();
            await eventEntryRepository.DeleteAllInStreamAsync(streamPath);
        }
    }
}
