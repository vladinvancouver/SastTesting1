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

        public static async Task DeleteAsync(string relativePath, string fileName)
        {
            await Task.CompletedTask;

            const string basePath = @"C:\Temp\";

            string filePath = System.IO.Path.Combine(basePath, NormalizePath(relativePath), NormalizePath(fileName));

            if (!File.Exists(filePath))
            {
                throw new Exception($"File not found: '{filePath}'.");
            }

            System.IO.File.Delete(filePath);
        }

        public static async Task<Stream> DownloadAsync(string relativePath, string fileName)
        {
            await Task.CompletedTask;

            const string basePath = @"C:\Temp\";

            string filePath = System.IO.Path.Combine(basePath, relativePath, fileName);

            if (!File.Exists(filePath))
            {
                throw new Exception($"File not found: '{filePath}'.");
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }


        public static string NormalizePath(string? path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return String.Empty;
            }

            string normalizedPath = path.Trim();
            normalizedPath = normalizedPath.Replace(@"\", "/");

            while (normalizedPath.StartsWith("/"))
            {
                normalizedPath = normalizedPath.Substring(1);
            }

            while (normalizedPath.EndsWith("/"))
            {
                normalizedPath = normalizedPath.Remove(normalizedPath.Length - 1);
            }

            normalizedPath = String.Join('/', normalizedPath.Split(new char[] { '/' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

            return normalizedPath;
        }
    }
}
