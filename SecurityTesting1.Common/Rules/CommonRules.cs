using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SecurityTesting1.Common.Rules
{
    public static class CommonRules
    {
        /// <summary>
        /// Linear sequence. For example: 1, 2, 3, 4, 5
        /// </summary>
        public static TimeSpan LinearSleepFunction(int retryAttempt)
        {
            return TimeSpan.FromSeconds(retryAttempt);
        }

        /// <summary>
        /// Exponential sequence to the power of 2. Example: 2, 4, 8, 16, 32
        /// </summary>
        public static TimeSpan ExponentialSleepFunction(int retryAttempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        }

        public static string CalculateMd5(string text)
        {
            return CalculateMd5(Encoding.UTF8.GetBytes(text));
        }

        public static string CalculateMd5(ReadOnlyMemory<byte> buffer)
        {
            return CalculateMd5(buffer.Span);
        }

        public static string CalculateMd5(byte[] buffer)
        {
            return CalculateMd5(buffer.AsSpan());
        }

        public static string CalculateMd5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                stream.Position = 0;
                byte[] hashValue = md5.ComputeHash(stream);
                string checksum = String.Empty;
                foreach (byte b in hashValue) checksum += b.ToString("x2");
                return checksum;
            }
        }

        public static string CalculateMd5(ReadOnlySpan<byte> buffer)
        {
            byte[] hashValue = MD5.HashData(buffer);
            string checksum = String.Empty;
            foreach (byte b in hashValue) checksum += b.ToString("x2");
            return checksum;
        }

        public static string CalculateSHA256(string text)
        {
            return CalculateSHA256(Encoding.UTF8.GetBytes(text));
        }

        public static string CalculateSHA256(byte[] buffer)
        {
            using (SHA256 sha256Alg = SHA256.Create())
            {
                byte[] hashValue = sha256Alg.ComputeHash(buffer);
                return Convert.ToBase64String(hashValue);
            }
        }

        public static byte[] GZipCompress(ReadOnlySpan<byte> data)
        {
            using (var to = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(to, CompressionLevel.Optimal, leaveOpen: true))
                    gZipStream.Write(data);

                return to.ToArray();
            }
        }

        public static byte[] GZipDecompress(byte[] data)
        {
            using (MemoryStream from = new MemoryStream(data))
            using (GZipStream gZipStream = new GZipStream(from, CompressionMode.Decompress))
            using (MemoryStream to = new())
            {
                gZipStream.CopyTo(to);
                return to.ToArray();
            }
        }

        /// <summary>
        /// Generate a self-signed JSON Web Token.
        /// </summary>
        public static string GenerateJwt(string subject, string issuer, string audience, string secretKey, TimeSpan duration)
        {
            ////TODO - move code to generate JWTs into a NuGet.

            //Source code for JsonWebTokenHandler
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/dev/src/Microsoft.IdentityModel.JsonWebTokens/JsonWebTokenHandler.cs

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new Exception($"Argument '{nameof(subject)}' is required to create a JWT.");
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new Exception($"Argument '{nameof(issuer)}' is required to create a JWT.");
            }

            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new Exception($"Argument '{nameof(audience)}' is required to create a JWT.");
            }

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new Exception($"Argument '{nameof(secretKey)}' is required to create a JWT.");
            }

            DateTime nowUtcDate = DateTime.UtcNow;
            DateTime expiresUtcDate = nowUtcDate.Add(duration);
            byte[] bytes = Encoding.UTF8.GetBytes(secretKey);
            SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(bytes), SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, subject) }),
                Issuer = issuer,
                Audience = audience,
                Expires = expiresUtcDate,
                IssuedAt = nowUtcDate,
                NotBefore = nowUtcDate,
                SigningCredentials = signingCredentials
            };
            JsonWebTokenHandler jsonWebTokenHandler = new();
            return jsonWebTokenHandler.CreateToken(tokenDescriptor);
        }

        public static IEnumerable<Claim> ParseJwt(string token)
        {
            Dictionary<string, object> claims = new();
            JsonWebToken jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
            return jwt.Claims.Where(obj => obj is { });
        }

        public static string SerializeUserDefinedFields(IDictionary<string, object> userDefinedFields, JsonSerializerOptions jsonSerializerOptions)
        {
            foreach (KeyValuePair<string, object> kvp in userDefinedFields)
            {
                if (kvp.Value.GetType().Name == "JsonElement")
                {
                    throw new Exception($"Will not serialize user defined fields because a data type was not inferred correctly for key '{kvp.Key}' and value '{kvp.Value}'.");
                }
            }

            return JsonSerializer.Serialize(userDefinedFields, userDefinedFields.GetType(), jsonSerializerOptions);
        }

        public static IDictionary<string, object> DeserializeUserDefinedFields(string serializedUserDefinedFields, JsonSerializerOptions jsonSerializerOptions)
        {
            if (string.IsNullOrWhiteSpace(serializedUserDefinedFields))
            {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            IDictionary<string, object>? userDefinedFields = JsonSerializer.Deserialize<IDictionary<string, object>>(serializedUserDefinedFields, jsonSerializerOptions);
            if (userDefinedFields is null)
            {
                throw new Exception($"Failed to deserialize user defined fields: '{serializedUserDefinedFields}'.");
            }

            foreach (KeyValuePair<string, object> kvp in userDefinedFields)
            {
                if (kvp.Value.GetType().Name == "JsonElement")
                {
                    throw new Exception($"Deserializing user defined fields failed to infer a data type for key '{kvp.Key}' and value '{kvp.Value}'.");
                }
            }

            return new Dictionary<string, object>(userDefinedFields, StringComparer.OrdinalIgnoreCase);
        }

        public static object GetUserDefinedField(IDictionary<string, object> userDefinedFields, string userDefinedFieldName)
        {
            userDefinedFields = new Dictionary<string, object>(userDefinedFields, StringComparer.OrdinalIgnoreCase);
            if (userDefinedFields.TryGetValue(userDefinedFieldName, out object? value))
            {
                return value;
            }

            throw new Exception($"User defined field '{userDefinedFieldName}' not found.");
        }

        /// <summary>
        /// Validate relative path.
        /// </summary>
        public static void ValidatePath(string path)
        {
            string normalizedPath = NormalizePath(path);

            string[] parts = normalizedPath.Split(new char[] { '\\', '/' });

            //Follow similar rules to Windows in case directories need to be moved into Windows
            List<string> invalidNames = new() { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

            foreach (string part in parts)
            {
                foreach (string invalidName in invalidNames)
                {
                    if (part == invalidName)
                    {
                        throw new Exception($"Argument '{nameof(path)}' contains a path part '{invalidName}' which is not allowed.");
                    }
                }

                if (String.IsNullOrWhiteSpace(part))
                {
                    throw new Exception($"Argument '{nameof(path)}' contains a path part that is empty.");
                }

                if (part.Trim().StartsWith('.'))
                {
                    throw new Exception($"Argument '{nameof(path)}' contains a path part starts with a period.");
                }

                if (part.Trim().EndsWith('.'))
                {
                    throw new Exception($"Argument '{nameof(path)}' contains a path part ends with a period.");
                }

                foreach (char character in part)
                {
                    if (Path.GetInvalidPathChars().Any(obj => obj == character))
                    {
                        throw new Exception($"Argument '{nameof(path)}' contains invalid character '{character}'.");
                    }

                    if (Path.GetInvalidFileNameChars().Any(obj => obj == character))
                    {
                        throw new Exception($"Argument '{nameof(path)}' contains invalid character '{character}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Clean and format a relative path.
        /// </summary>
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

        /// <summary>
        /// Combine relative paths.
        /// </summary>
        public static string CombineAndNormalizePath(params string[] paths)
        {
            List<string> normalizedPaths = new();
            foreach (string path in paths)
            {
                if (!String.IsNullOrWhiteSpace(path))
                {
                    normalizedPaths.Add(NormalizePath(path));
                }
            }
            normalizedPaths.RemoveAll(obj => String.IsNullOrWhiteSpace(obj));
            string normalizedPath = String.Join('/', normalizedPaths);

            return normalizedPath;
        }

        public static string GetRunningAsUser()
        {
            if (System.OperatingSystem.IsWindows())
            {
                return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }

            if (!String.IsNullOrWhiteSpace(System.Environment.UserName))
            {
                if (String.IsNullOrWhiteSpace(System.Environment.UserDomainName))
                {
                    return System.Environment.UserName;
                }
                else
                {
                    return $"{System.Environment.UserDomainName}\\{System.Environment.UserName}";
                }
            }

            if (!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("USERNAME")))
            {
                return Environment.GetEnvironmentVariable("USERNAME")!;
            }

            if (!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("USER")))
            {
                return Environment.GetEnvironmentVariable("USER")!;
            }

            return "[Unknown]";
        }
    }
}
