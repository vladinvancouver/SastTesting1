using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecurityTesting1.DataAccess.Objects;

namespace SecurityTesting1.DataAccess.Databases
{
    internal static class AccountDatabase
    {
        internal static DataAccess.Objects.Account? GetAccountById(string connectionString, Guid accountId)
        {
            string sql = @"SELECT
                        [AccountId]
                        ,[AccountName]
                        ,[Description]
                        ,[IsActive]
                        ,[IsMarkedForDeletion]
                        ,[CreatedFromRemoteIpAddress]
                        ,[CreatedBy]
                        ,[CreatedUtcDate]
                        ,[CreatedUserAgent]
                        ,[UpdatedFromRemoteIpAddress]
                        ,[UpdatedBy]
                        ,[UpdatedUtcDate]
                        ,[UpdatedUserAgent]
                    FROM
                        [dbo].[Accounts]
                    WHERE
                        [AccountId] = @accountId;";
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("accountId", accountId);

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRowToAccount(reader);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        internal static DataAccess.Objects.Account? GetAccountByAccountName(string connectionString, string accountName)
        {
            string sql = @"SELECT
                        [AccountId]
                        ,[AccountName]
                        ,[Description]
                        ,[IsActive]
                        ,[IsMarkedForDeletion]
                        ,[CreatedFromRemoteIpAddress]
                        ,[CreatedBy]
                        ,[CreatedUtcDate]
                        ,[CreatedUserAgent]
                        ,[UpdatedFromRemoteIpAddress]
                        ,[UpdatedBy]
                        ,[UpdatedUtcDate]
                        ,[UpdatedUserAgent]
                    FROM
                        [dbo].[Accounts]
                    WHERE
                        [AccountName] = '" + accountName + "';";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                //cmd.Parameters.AddWithValue("accountName", accountName);

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRowToAccount(reader);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }


        internal static IEnumerable<DataAccess.Objects.Account> GetAllAccounts(string connectionString)
        {
            List<DataAccess.Objects.Account> data = new List<Objects.Account>();
            string sql = @"SELECT
                        [AccountId]
                        ,[AccountName]
                        ,[Description]
                        ,[IsActive]
                        ,[IsMarkedForDeletion]
                        ,[CreatedFromRemoteIpAddress]
                        ,[CreatedBy]
                        ,[CreatedUtcDate]
                        ,[CreatedUserAgent]
                        ,[UpdatedFromRemoteIpAddress]
                        ,[UpdatedBy]
                        ,[UpdatedUtcDate]
                        ,[UpdatedUserAgent]
                    FROM
                        [dbo].[Accounts]
                    ORDER BY
                        [AccountName] ASC;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(MapRowToAccount(reader));
                    }

                    return data;
                }
            }
        }

        internal static void AddAccount(string connectionString, DataAccess.Objects.Account account)
        {
            string sql = @"INSERT INTO
                                [dbo].[Accounts]
                            ([AccountId]
                            ,[AccountName]
                            ,[Description]
                            ,[IsActive]
                            ,[IsMarkedForDeletion]
                            ,[CreatedFromRemoteIpAddress]
                            ,[CreatedBy]
                            ,[CreatedUtcDate]
                            ,[CreatedUserAgent]
                            ,[UpdatedFromRemoteIpAddress]
                            ,[UpdatedBy]
                            ,[UpdatedUtcDate]
                            ,[UpdatedUserAgent])
                        VALUES
                            (@accountId
                            ,@accountName
                            ,@description
                            ,@isActive
                            ,@isMarkedForDeletion
                            ,@createdFromRemoteIpAddress
                            ,@createdBy
                            ,@createdUtcDate
                            ,@createdUserAgent
                            ,@updatedFromRemoteIpAddress
                            ,@updatedBy
                            ,@updatedUtcDate
                            ,@updatedUserAgent);";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("accountId", account.AccountId);
                cmd.Parameters.AddWithValue("accountName", new String(account.AccountName.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("description", new String(account.Description.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("isActive", account.IsActive);
                cmd.Parameters.AddWithValue("isMarkedForDeletion", account.IsMarkedForDeletion);
                cmd.Parameters.AddWithValue("createdFromRemoteIpAddress", new String(account.CreatedFromRemoteIpAddress.Trim().Take(45).ToArray()));
                cmd.Parameters.AddWithValue("createdBy", new String(account.CreatedBy.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("createdUtcDate", account.CreatedUtcDate);
                cmd.Parameters.AddWithValue("createdUserAgent", new String(account.CreatedUserAgent.Trim().Take(200).ToArray()));
                cmd.Parameters.AddWithValue("updatedFromRemoteIpAddress", new String(account.UpdatedFromRemoteIpAddress.Trim().Take(45).ToArray()));
                cmd.Parameters.AddWithValue("updatedBy", new String(account.UpdatedBy.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("updatedUtcDate", account.UpdatedUtcDate);
                cmd.Parameters.AddWithValue("updatedUserAgent", new String(account.UpdatedUserAgent.Trim().Take(200).ToArray()));

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        internal static void UpdateAccount(string connectionString, DataAccess.Objects.Account account)
        {
            string sql = @"UPDATE
                                [dbo].[Accounts]
                           SET
                                [AccountName] = @accountName
                              ,[Description] = @description
                              ,[IsActive] = @isActive
                              ,[IsMarkedForDeletion] = @isMarkedForDeletion
                              ,[CreatedFromRemoteIpAddress] = @createdFromRemoteIpAddress
                              ,[CreatedBy] = @createdBy
                              ,[CreatedUtcDate] = @createdUtcDate
                              ,[CreatedUserAgent] = @createdUserAgent
                              ,[UpdatedFromRemoteIpAddress] = @updatedFromRemoteIpAddress
                              ,[UpdatedBy] = @updatedBy
                              ,[UpdatedUtcDate] = @updatedUtcDate
                              ,[UpdatedUserAgent] = @updatedUserAgent
                        WHERE
                            [AccountId] = @accountId;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("accountId", account.AccountId);
                cmd.Parameters.AddWithValue("accountName", new String(account.AccountName.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("description", new String(account.Description.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("isActive", account.IsActive);
                cmd.Parameters.AddWithValue("isMarkedForDeletion", account.IsMarkedForDeletion);
                cmd.Parameters.AddWithValue("createdFromRemoteIpAddress", new String(account.CreatedFromRemoteIpAddress.Trim().Take(45).ToArray()));
                cmd.Parameters.AddWithValue("createdBy", new String(account.CreatedBy.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("createdUtcDate", account.CreatedUtcDate);
                cmd.Parameters.AddWithValue("createdUserAgent", new String(account.CreatedUserAgent.Trim().Take(200).ToArray()));
                cmd.Parameters.AddWithValue("updatedFromRemoteIpAddress", new String(account.UpdatedFromRemoteIpAddress.Trim().Take(45).ToArray()));
                cmd.Parameters.AddWithValue("updatedBy", new String(account.UpdatedBy.Trim().Take(100).ToArray()));
                cmd.Parameters.AddWithValue("updatedUtcDate", account.UpdatedUtcDate);
                cmd.Parameters.AddWithValue("updatedUserAgent", new String(account.UpdatedUserAgent.Trim().Take(200).ToArray()));

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        internal static void DeleteAccount(string connectionString, Guid id)
        {
            string sql = @"DELETE FROM
                                [dbo].[Accounts]
                            WHERE
                                [AccountId] = @accountId;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("accountId", id);

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        private static DataAccess.Objects.Account MapRowToAccount(SqlDataReader reader)
        {
            DataAccess.Objects.Account a = new DataAccess.Objects.Account();
            a.AccountId = reader.GetGuid(reader.GetOrdinal("AccountId"));
            a.AccountName = reader.GetString(reader.GetOrdinal("AccountName"));
            a.Description = reader.GetString(reader.GetOrdinal("Description"));
            a.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
            a.IsMarkedForDeletion = reader.GetBoolean(reader.GetOrdinal("IsMarkedForDeletion"));
            a.CreatedFromRemoteIpAddress = reader.GetString(reader.GetOrdinal("CreatedFromRemoteIpAddress"));
            a.CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy"));
            a.CreatedUtcDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("CreatedUtcDate")), DateTimeKind.Utc);
            a.CreatedUserAgent = reader.GetString(reader.GetOrdinal("CreatedUserAgent"));
            a.UpdatedFromRemoteIpAddress = reader.GetString(reader.GetOrdinal("UpdatedFromRemoteIpAddress"));
            a.UpdatedBy = reader.GetString(reader.GetOrdinal("UpdatedBy"));
            a.UpdatedUtcDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("UpdatedUtcDate")), DateTimeKind.Utc);
            a.UpdatedUserAgent = reader.GetString(reader.GetOrdinal("UpdatedUserAgent"));
            return a;
        }
    }
}
