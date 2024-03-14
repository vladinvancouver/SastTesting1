using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityTesting1.DataAccess.Objects
{
    public class EventEntry
    {
        public long EventEntryId { get; set; }
        public string StreamPath { get; set; } = String.Empty;
        public byte[] Data { get; set; } = new byte[] { };
        public string DataType { get; set; } = String.Empty;
        public CompressionType CompressionType { get; set; } = CompressionType.None;
        public SerializationType SerializationType { get; set; } = SerializationType.JSON;


        /// <summary>
        /// Date the event occurred
        /// </summary>
        public DateTime OccurredUtcDate { get; set; }

        /// <summary>
        /// In some domains, like accounting, there is a difference when an event occurred and when the event is noticed.
        /// </summary>
        public DateTime NoticedUtcDate { get; set; }

        /// <summary>
        /// Date the event was received.
        /// </summary>
        public DateTime ReceivedUtcDate { get; set; }

        /// <summary>
        /// Date the event was saved to storage.
        /// </summary>
        public DateTime PersistedUtcDate { get; set; }

        public string CreatedBy { get; set; } = String.Empty;
        public string CreatedFromRemoteIpAddress { get; set; } = String.Empty;
        public string CreatedUserAgent { get; set; } = String.Empty;
    }
}
