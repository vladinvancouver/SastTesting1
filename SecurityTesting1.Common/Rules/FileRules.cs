using Company.Utilities;
using SecurityTesting1.Common.AuditEvents;
using SecurityTesting1.Common.Objects;
using SecurityTesting1.Common.Services;
using SecurityTesting1.DataAccess.Repositories.AccountRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace SecurityTesting1.Common.Rules
{
    public static class FileRules
    {
        public static async Task<IEnumerable<DataTransfer.Objects.File>> GetAllAsync(string relativePath)
        {
            await Task.CompletedTask;

            const string basePath = @"C:\Temp\";

            List<DataTransfer.Objects.File> results = new();

            foreach (var item in System.IO.Directory.GetFiles(System.IO.Path.Combine(basePath, relativePath)))
            {
                System.IO.FileInfo fileInfo = new FileInfo(item);

                results.Add(new DataTransfer.Objects.File()
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length
                });
            }

            return results;
        }
    }
}
