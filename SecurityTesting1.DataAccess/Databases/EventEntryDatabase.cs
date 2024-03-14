using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Data;
using SecurityTesting1.DataAccess.Objects;
using Company.Utilities;

namespace SecurityTesting1.DataAccess.Databases
{
    internal static class EventEntryDatabase
    {
        internal static void AddEventEntry(string connectionString, DataAccess.Objects.EventEntry eventEntry)
        {
            string sql = @"INSERT INTO
                                [dbo].[EventEntries]
                            ([StreamPath]
                            ,[Data]
                            ,[DataType]
                            ,[CompressionType]
                            ,[SerializationType]
                            ,[OccurredUtcDate]
                            ,[NoticedUtcDate]
                            ,[ReceivedUtcDate]
                            ,[PersistedUtcDate]
                            ,[CreatedBy]
                            ,[CreatedFromRemoteIpAddress]
                            ,[CreatedUserAgent])
                        OUTPUT
                            inserted.[EventEntryId]
                        VALUES
                            (@streamPath
                            ,@data
                            ,@dataType
                            ,@compressionType
                            ,@serializationType
                            ,@occurredUtcDate
                            ,@noticedUtcDate
                            ,@receivedUtcDate
                            ,@persistedUtcDate
                            ,@createdBy
                            ,@createdFromRemoteIpAddress
                            ,@createdUserAgent);";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("streamPath", new String(eventEntry.StreamPath.Trim().Take(400).ToArray()));
                cmd.Parameters.AddWithValue("data", eventEntry.Data);
                cmd.Parameters.AddWithValue("dataType", new String(eventEntry.DataType.Trim().Take(400).ToArray()));
                cmd.Parameters.AddWithValue("compressionType", new String(eventEntry.CompressionType.ToString().Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("serializationType", new String(eventEntry.SerializationType.ToString().Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("occurredUtcDate", eventEntry.OccurredUtcDate);
                cmd.Parameters.AddWithValue("noticedUtcDate", eventEntry.NoticedUtcDate);
                cmd.Parameters.AddWithValue("receivedUtcDate", eventEntry.ReceivedUtcDate);
                cmd.Parameters.AddWithValue("persistedUtcDate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("createdBy", new String(eventEntry.CreatedBy.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("createdFromRemoteIpAddress", new String(eventEntry.CreatedFromRemoteIpAddress.Trim().Take(45).ToArray()));
                cmd.Parameters.AddWithValue("createdUserAgent", new String(eventEntry.CreatedUserAgent.Trim().Take(200).ToArray()));

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                eventEntry.EventEntryId = (long)cmd.ExecuteScalar();
            }
        }

        internal static void AddEventEntries(string connectionString, IEnumerable<Objects.EventEntry> eventEntries)
        {
            const int BATCH_SIZE = 10;

            foreach (IEnumerable<Objects.EventEntry> batch in eventEntries.Batch(BATCH_SIZE))
            {
                List<Objects.EventEntry> range = batch.ToList();

                StringBuilder sb = new StringBuilder();
                sb.Append(@"INSERT INTO
                                [dbo].[EventEntries]
                            ([StreamPath]
                            ,[Data]
                            ,[DataType]
                            ,[CompressionType]
                            ,[SerializationType]
                            ,[OccurredUtcDate]
                            ,[NoticedUtcDate]
                            ,[ReceivedUtcDate]
                            ,[PersistedUtcDate]
                            ,[CreatedBy]
                            ,[CreatedFromRemoteIpAddress]
                            ,[CreatedUserAgent])
                        OUTPUT
                            inserted.[EventEntryId]
                        VALUES ");

                int batchNumber = 0;
                Dictionary<string, object> values = new Dictionary<string, object>();
                foreach (Objects.EventEntry eventEntry in range)
                {
                    batchNumber++;

                    if (batchNumber > 1)
                        sb.Append(", ");

                    sb.Append("(");
                    sb.Append("@streamPath" + batchNumber);
                    sb.Append(",@data" + batchNumber);
                    sb.Append(",@dataType" + batchNumber);
                    sb.Append(",@compressionType" + batchNumber);
                    sb.Append(",@serializationType" + batchNumber);
                    sb.Append(",@occurredUtcDate" + batchNumber);
                    sb.Append(",@noticedUtcDate" + batchNumber);
                    sb.Append(",@receivedUtcDate" + batchNumber);
                    sb.Append(",@persistedUtcDate" + batchNumber);
                    sb.Append(",@createdBy" + batchNumber);
                    sb.Append(",@createdFromRemoteIpAddress" + batchNumber);
                    sb.Append(",@createdUserAgent" + batchNumber);
                    sb.Append(")");

                    values.Add("streamPath" + batchNumber, new String(eventEntry.StreamPath.Trim().Take(400).ToArray()));
                    values.Add("data" + batchNumber, eventEntry.Data);
                    values.Add("dataType" + batchNumber, new String(eventEntry.DataType.Trim().Take(400).ToArray()));
                    values.Add("compressionType" + batchNumber, new String(eventEntry.CompressionType.ToString().Trim().Take(100).ToArray()));
                    values.Add("serializationType" + batchNumber, new String(eventEntry.SerializationType.ToString().Trim().Take(100).ToArray()));
                    values.Add("occurredUtcDate" + batchNumber, eventEntry.OccurredUtcDate);
                    values.Add("noticedUtcDate" + batchNumber, eventEntry.NoticedUtcDate);
                    values.Add("receivedUtcDate" + batchNumber, eventEntry.ReceivedUtcDate);
                    values.Add("persistedUtcDate" + batchNumber, DateTime.UtcNow);
                    values.Add("createdBy" + batchNumber, new String(eventEntry.CreatedBy.Trim().Take(100).ToArray()));
                    values.Add("createdFromRemoteIpAddress" + batchNumber, new String(eventEntry.CreatedFromRemoteIpAddress.Trim().Take(45).ToArray()));
                    values.Add("createdUserAgent" + batchNumber, new String(eventEntry.CreatedUserAgent.Trim().Take(200).ToArray()));
                }
                sb.Append(";");

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                {
                    foreach (KeyValuePair<string, object> kvp in values)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }

                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 0;
                        while (reader.Read())
                        {
                            range[index].EventEntryId = reader.GetInt64(reader.GetOrdinal("EventEntryId"));
                            index++;


                        }
                    }
                }
            }
        }

        internal static void AddEventEntriesInBulk(string connectionString, IEnumerable<Objects.EventEntry> eventEntries)
        {
            const int BATCH_SIZE = 100_000;

            System.Data.DataTable dataTable = new DataTable("EventEntries");

            DataColumn column;

            column = new DataColumn();
            column.DataType = typeof(Int64);
            column.ColumnName = "EventEntryId";
            column.ReadOnly = true;
            column.Unique = true;
            column.AutoIncrement = true;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "StreamPath";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(byte[]);
            column.ColumnName = "Data";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "DataType";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "CompressionType";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "SerializationType";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(DateTime);
            column.ColumnName = "OccurredUtcDate";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(DateTime);
            column.ColumnName = "NoticedUtcDate";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(DateTime);
            column.ColumnName = "ReceivedUtcDate";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(DateTime);
            column.ColumnName = "PersistedUtcDate";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "CreatedBy";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "CreatedFromRemoteIpAddress";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "CreatedUserAgent";
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = false;
            dataTable.Columns.Add(column);

            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = dataTable.Columns["EventEntryId"]!;
            dataTable.PrimaryKey = PrimaryKeyColumns;

            foreach (EventEntry eventEntry in eventEntries)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["StreamPath"] = new String(eventEntry.StreamPath.Trim().Take(400).ToArray());
                dataRow["Data"] = eventEntry.Data;
                dataRow["DataType"] = new String(eventEntry.DataType.Trim().Take(400).ToArray());
                dataRow["CompressionType"] = new String(eventEntry.CompressionType.ToString().Trim().Take(100).ToArray());
                dataRow["SerializationType"] = new String(eventEntry.SerializationType.ToString().Trim().Take(100).ToArray());
                dataRow["OccurredUtcDate"] = eventEntry.OccurredUtcDate;
                dataRow["NoticedUtcDate"] = eventEntry.NoticedUtcDate;
                dataRow["ReceivedUtcDate"] = eventEntry.ReceivedUtcDate;
                dataRow["PersistedUtcDate"] = DateTime.UtcNow;
                dataRow["CreatedBy"] = new String(eventEntry.CreatedBy.Trim().Take(100).ToArray());
                dataRow["CreatedFromRemoteIpAddress"] = new String(eventEntry.CreatedFromRemoteIpAddress.Trim().Take(45).ToArray());
                dataRow["CreatedUserAgent"] = new String(eventEntry.CreatedUserAgent.Trim().Take(200).ToArray());

                dataTable.Rows.Add(dataRow);
            }

            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionString))
            {
                sqlBulkCopy.BulkCopyTimeout = TimeSpan.FromSeconds(120).Seconds;
                sqlBulkCopy.DestinationTableName = "dbo.EventEntries";
                sqlBulkCopy.BatchSize = BATCH_SIZE;
                sqlBulkCopy.WriteToServer(dataTable);
                sqlBulkCopy.Close();
            }
        }


        internal static DataAccess.Objects.EventEntry? GetNextEventEntrySinceLastSequenceNumber(string connectionString, IEnumerable<string> streamPaths, long lastSequenceNumber)
        {
            return GetNextEventEntryBatchSinceLastSequenceNumber(connectionString, streamPaths, lastSequenceNumber, maxCount: 1).FirstOrDefault();
        }

        internal static IEnumerable<DataAccess.Objects.EventEntry> GetNextEventEntryBatchSinceLastSequenceNumber(string connectionString, IEnumerable<string> streamPaths, long lastSequenceNumber, int maxCount)
        {
            List<DataAccess.Objects.EventEntry> results = new();

            StringBuilder sb = new StringBuilder();
            sb.Append($@"SELECT TOP {maxCount}
                            [EventEntryId]
                            ,[StreamPath]
                            ,[Data]
                            ,[DataType]
                            ,[CompressionType]
                            ,[SerializationType]
                            ,[OccurredUtcDate]
                            ,[NoticedUtcDate]
                            ,[ReceivedUtcDate]
                            ,[PersistedUtcDate]
                            ,[CreatedBy]
                            ,[CreatedFromRemoteIpAddress]
                            ,[CreatedUserAgent]
                    FROM
                        [dbo].[EventEntries]

                        WHERE
                            (");

            int streamPathNumber = 0;
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (string streamPath in streamPaths)
            {
                string value = streamPath;
                if (value.EndsWith("/"))
                {
                    value = value + "%";
                }
                else
                {
                    value = value + "/%";
                }

                streamPathNumber++;

                if (streamPathNumber > 1)
                    sb.Append(" OR ");

                sb.Append("[StreamPath] LIKE @streamPath" + streamPathNumber);

                values.Add("streamPath" + streamPathNumber, new String(value.Trim().Take(400).ToArray()));
            }

            sb.Append(") ");
            sb.Append(@"AND [EventEntryId] > @lastSequenceNumber
                        ORDER BY
                            [EventEntryId] ASC;");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
            {
                foreach (KeyValuePair<string, object> kvp in values)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
                cmd.Parameters.AddWithValue("lastSequenceNumber", lastSequenceNumber);

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                //SequentialAccess may speed up accessing blobs
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        results.Add(MapRowToEventEntry(reader));
                    }

                    return results;
                }
            }
        }

        internal static long GetLastSequenceNumber(string connectionString, IEnumerable<string> streamPaths)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($@"SELECT TOP 1
                              [EventEntryId]
                          FROM
                                [dbo].[EventEntries]
                        WHERE
                            (");

            int streamPathNumber = 0;
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (string streamPath in streamPaths)
            {
                string value = streamPath;
                if (value.EndsWith("/"))
                {
                    value = value + "%";
                }
                else
                {
                    value = value + "/%";
                }

                streamPathNumber++;

                if (streamPathNumber > 1)
                    sb.Append(" OR ");

                sb.Append("[StreamPath] LIKE @streamPath" + streamPathNumber);

                values.Add("streamPath" + streamPathNumber, new String(value.Trim().Take(400).ToArray()));
            }

            sb.Append($@")
                    ORDER BY
                        [EventEntryId] DESC;");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
            {
                foreach (KeyValuePair<string, object> kvp in values)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                //SequentialAccess may speed up accessing blobs
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    if (reader.Read())
                    {
                        return reader.GetInt64(reader.GetOrdinal("EventEntryId"));
                    }

                    return 0;
                }
            }
        }

        internal static void DeleteEventEntriesInStream(string connectionString, string streamPath)
        {
            if (streamPath.EndsWith("/"))
            {
                streamPath = streamPath + "%";
            }
            else
            {
                streamPath = streamPath + "/%";
            }

            string sql = @"DELETE FROM
                                [dbo].[EventEntries]
                            WHERE
                                [StreamPath] LIKE @streamPath;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                cmd.Parameters.AddWithValue("streamPath", streamPath);

                cmd.ExecuteNonQuery();
            }
        }


        private static DataAccess.Objects.EventEntry MapRowToEventEntry(SqlDataReader reader)
        {
            DataAccess.Objects.EventEntry ee = new DataAccess.Objects.EventEntry();
            ee.EventEntryId = reader.GetInt64(reader.GetOrdinal("EventEntryId"));
            ee.StreamPath = reader.GetString(reader.GetOrdinal("StreamPath"));
            ee.Data = (byte[])reader["Data"];
            ee.DataType = reader.GetString(reader.GetOrdinal("DataType"));
            ee.CompressionType = Enum.Parse<CompressionType>(reader.GetString(reader.GetOrdinal("CompressionType")));
            ee.SerializationType = Enum.Parse<SerializationType>(reader.GetString(reader.GetOrdinal("SerializationType")));
            ee.OccurredUtcDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("OccurredUtcDate")), DateTimeKind.Utc);
            ee.NoticedUtcDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("NoticedUtcDate")), DateTimeKind.Utc);
            ee.ReceivedUtcDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("ReceivedUtcDate")), DateTimeKind.Utc);
            ee.PersistedUtcDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("PersistedUtcDate")), DateTimeKind.Utc);
            ee.CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy"));
            ee.CreatedFromRemoteIpAddress = reader.GetString(reader.GetOrdinal("CreatedFromRemoteIpAddress"));
            ee.CreatedUserAgent = reader.GetString(reader.GetOrdinal("CreatedUserAgent"));
            return ee;
        }
    }
}
