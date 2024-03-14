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
    public static class AccountRules
    {
        public static async Task<DataTransfer.Objects.Account?> GetByIdAsync(StorageService storageService, JsonSerializerOptions jsonSerializerOptions, Guid accountId)
        {
            IAccountRepository accountRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IAccountRepository>();

            DataAccess.Objects.Account? da = await accountRepository.GetByIdAsync(accountId);
            if (da is null)
            {
                return null;
            }

            return new DataTransfer.Objects.Account()
            {
                AccountId = da.AccountId,
                AccountName = da.AccountName,
                Description = da.Description,
                IsActive = da.IsActive
            };
        }

        public static async Task<IEnumerable<DataTransfer.Objects.Account>> GetAllAsync(StorageService storageService, JsonSerializerOptions jsonSerializerOptions)
        {
            IAccountRepository accountRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IAccountRepository>();

            List<DataTransfer.Objects.Account> results = new();
            IEnumerable<DataAccess.Objects.Account> daAccounts = await accountRepository.GetAllAsync();
            foreach (DataAccess.Objects.Account da in daAccounts.Where(obj => !obj.IsMarkedForDeletion))
            {
                results.Add(new DataTransfer.Objects.Account()
                {
                    AccountId = da.AccountId,
                    AccountName = da.AccountName,
                    Description = da.Description,
                    IsActive = da.IsActive
                });
            }

            return results;
        }

        public static async Task<(Guid AccountId, bool IsCreated)> GetOrCreateAccount(StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, string accountName, string performedBy, string fromRemoteIpAddress, string userAgent)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted; //Required for READPAST hint
            //Use TransactionScopeAsyncFlowOption.Enabled in async code.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                DateTime utcNow = SystemTime.UtcNow;
                IAccountRepository txAccountRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IAccountRepository>();

                //Account Name must be unique. We are going to try to create an account and if it fails, we are going
                //to _assume_ that it is because an account with that name already exists.
                try
                {
                    DataAccess.Objects.Account account = new DataAccess.Objects.Account()
                    {
                        AccountId = Guid.NewGuid(),
                        AccountName = accountName,
                        IsActive = true,
                        IsMarkedForDeletion = false,
                        CreatedFromRemoteIpAddress = fromRemoteIpAddress,
                        CreatedUtcDate = utcNow,
                        CreatedBy = performedBy,
                        CreatedUserAgent = userAgent,
                        UpdatedFromRemoteIpAddress = fromRemoteIpAddress,
                        UpdatedUtcDate = utcNow,
                        UpdatedBy = performedBy,
                        UpdatedUserAgent = userAgent
                    };

                    await txAccountRepository.AddAsync(account);

                    scope.Complete();

                    //Create event
                    AccountCreatedAuditEvent001 accountCreatedAuditEvent = new()
                    {
                        AccountId = account.AccountId,
                        AccountName = account.AccountName,
                        IsActive = account.IsActive,
                        IsMarkedForDeletion = account.IsMarkedForDeletion,
                        FromRemoteIpAddress = fromRemoteIpAddress,
                        OccurredUtcDate = utcNow,
                        PerformedBy = performedBy,
                        UserAgent = userAgent
                    };

                    //Save event
                    string streamPath = "/Audits/Accounts/";
                    EventMessage eventMessage = new(streamPath, accountCreatedAuditEvent)
                    {
                        PerformedBy = performedBy,
                        FromRemoteIpAddress = fromRemoteIpAddress,
                        UserAgent = userAgent
                    };
                    eventService.Enqueue(eventMessage);

                    return (account.AccountId, IsCreated: true);
                }
                catch
                {
                    IAccountRepository accountRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IAccountRepository>();
                    DataAccess.Objects.Account? account = await accountRepository.GetByAccountNameAsync(accountName);
                    if (account is { })
                    {
                        return (account.AccountId, IsCreated: false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public static async Task<DataTransfer.Objects.Account> AddAccountAsync(StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, DataTransfer.Objects.Account account, string performedBy, string fromRemoteIpAddress, string userAgent)
        {
            if (string.IsNullOrWhiteSpace(account.AccountName))
                throw new ArgumentException($"'Account name' is blank.");

            if (account.AccountName.Length > 100)
                throw new ArgumentException($"'Account name' must be 100 characters or less.");

            if (string.IsNullOrWhiteSpace(account.Description))
                throw new ArgumentException($"'Description' is blank.");

            if (account.Description.Length > 100)
                throw new ArgumentException($"'Description' must be 100 characters or less.");

            IAccountRepository accountRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IAccountRepository>();

            if ((await accountRepository.GetAllAsync()).Any(obj => obj.AccountName.ToUpperInvariant() == account.AccountName.ToUpperInvariant()))
            {
                throw new ArgumentException($"Account name '{account.AccountName}' already in use.");
            }

            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted; //Required for READPAST hint
            //Use TransactionScopeAsyncFlowOption.Enabled in async code.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                DateTime utcNow = SystemTime.UtcNow;
                IAccountRepository txAccountRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IAccountRepository>();

                //Save account
                DataAccess.Objects.Account da = new DataAccess.Objects.Account()
                {
                    AccountId = Guid.NewGuid(),
                    AccountName = account.AccountName,
                    Description = account.Description,
                    IsActive = account.IsActive,
                    IsMarkedForDeletion = false,
                    CreatedFromRemoteIpAddress = fromRemoteIpAddress,
                    CreatedUtcDate = utcNow,
                    CreatedBy = performedBy,
                    CreatedUserAgent = userAgent,
                    UpdatedFromRemoteIpAddress = fromRemoteIpAddress,
                    UpdatedUtcDate = utcNow,
                    UpdatedBy = performedBy,
                    UpdatedUserAgent = userAgent
                };

                await txAccountRepository.AddAsync(da);

                scope.Complete();

                //Create event
                AccountCreatedAuditEvent001 accountCreatedAuditEvent = new()
                {
                    AccountId = da.AccountId,
                    AccountName = da.AccountName,
                    IsActive = da.IsActive,
                    IsMarkedForDeletion = da.IsMarkedForDeletion,
                    FromRemoteIpAddress = fromRemoteIpAddress,
                    OccurredUtcDate = utcNow,
                    PerformedBy = performedBy,
                    UserAgent = userAgent
                };

                //Save event
                string streamPath = "/Audits/Accounts/";
                EventMessage eventMessage = new(streamPath, accountCreatedAuditEvent)
                {
                    PerformedBy = performedBy,
                    FromRemoteIpAddress = fromRemoteIpAddress,
                    UserAgent = userAgent
                };
                eventService.Enqueue(eventMessage);

                //Return object
                DataTransfer.Objects.Account dt = new()
                {
                    AccountId = da.AccountId,
                    AccountName = da.AccountName,
                    IsActive = da.IsActive
                };

                return account;
            }
        }

        public static async Task UpdateAccountAsync(StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, DataTransfer.Objects.Account account, string performedBy, string fromRemoteIpAddress, string userAgent)
        {
            if (account == null)
                throw new ArgumentException($"Cannot read account.");

            if (string.IsNullOrWhiteSpace(account.AccountName))
                throw new ArgumentException($"'Account name' is blank.");

            if (account.AccountName.Length > 100)
                throw new ArgumentException($"'Account name' must be 100 characters or less.");

            IAccountRepository accountRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IAccountRepository>();

            IEnumerable<DataAccess.Objects.Account> accounts = await accountRepository.GetAllAsync();

            if (accounts.Any(o => o.AccountName.ToUpperInvariant() == account.AccountName.ToUpperInvariant() && o.AccountId != account.AccountId))
            {
                throw new ArgumentException($"Name '{account.AccountName}' already in use.");
            }

            DataAccess.Objects.Account? da = accounts.FirstOrDefault(o => o.AccountId == account.AccountId);

            if (da == null)
            {
                throw new ArgumentException($"Account '{account.AccountId}' not found.");
            }

            if (da.IsMarkedForDeletion)
            {
                throw new ArgumentException($"Cannot update because it was marked for deletion.");
            }

            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted; //Required for READPAST hint
            //Use TransactionScopeAsyncFlowOption.Enabled in async code.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                DateTime utcNow = SystemTime.UtcNow;
                IAccountRepository txAccountRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IAccountRepository>();

                //Save account
                da.AccountName = account.AccountName;
                da.Description = account.Description;
                da.IsActive = account.IsActive;
                da.UpdatedFromRemoteIpAddress = fromRemoteIpAddress;
                da.UpdatedUtcDate = utcNow;
                da.UpdatedBy = performedBy;
                da.UpdatedUserAgent = userAgent;

                await txAccountRepository.UpdateAsync(da);

                scope.Complete();

                //Create event
                AccountUpdatedAuditEvent001 accountUpdatedAuditEvent = new()
                {
                    AccountId = da.AccountId,
                    AccountName = da.AccountName,
                    IsActive = da.IsActive,
                    IsMarkedForDeletion = da.IsMarkedForDeletion,
                    FromRemoteIpAddress = fromRemoteIpAddress,
                    OccurredUtcDate = utcNow,
                    PerformedBy = performedBy,
                    UserAgent = userAgent
                };

                //Save event
                string streamPath = "/Audits/Accounts/";
                EventMessage eventMessage = new(streamPath, accountUpdatedAuditEvent)
                {
                    PerformedBy = performedBy,
                    FromRemoteIpAddress = fromRemoteIpAddress,
                    UserAgent = userAgent
                };
                eventService.Enqueue(eventMessage);
            }
        }

        public static async Task MarkAccountForDeletionAsync(StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, Guid accountId, string performedBy, string fromRemoteIpAddress, string userAgent)
        {
            IAccountRepository accountRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IAccountRepository>();
            DataAccess.Objects.Account? account = await accountRepository.GetByIdAsync(accountId);
            if (account is null)
            {
                return;
            }

            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted; //Required for READPAST hint
            //Use TransactionScopeAsyncFlowOption.Enabled in async code.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                DateTime utcNow = SystemTime.UtcNow;
                IAccountRepository txAccountRepository = storageService.ForUseWithTransactionsUnitOfWork.GetRepository<IAccountRepository>();

                //It takes time to permanently delete an account. Since account names are supposed to be unique,
                //change the account name.
                string accountName = account.AccountName;
                account.AccountName = accountName + $" ({Guid.NewGuid()})";
                account.IsMarkedForDeletion = true;
                account.UpdatedBy = performedBy;
                account.UpdatedUtcDate = utcNow;
                account.UpdatedFromRemoteIpAddress = fromRemoteIpAddress;
                account.UpdatedUserAgent = userAgent;
                await txAccountRepository.UpdateAsync(account);

                //Create event
                AccountDeletedAuditEvent001 accountDeletedAuditEvent = new()
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName,
                    FromRemoteIpAddress = fromRemoteIpAddress,
                    OccurredUtcDate = utcNow,
                    PerformedBy = performedBy,
                    UserAgent = userAgent
                };

                scope.Complete();

                //Save event
                string streamPath = "/Audits/Accounts/";
                EventMessage eventMessage = new(streamPath, accountDeletedAuditEvent)
                {
                    PerformedBy = performedBy,
                    FromRemoteIpAddress = fromRemoteIpAddress,
                    UserAgent = userAgent
                };
                eventService.Enqueue(eventMessage);
            }
        }

        public static async Task PermanentlyDeleteAccountsMarkedForDeletionAsync(StorageService storageService, TimeSpan wait, CancellationToken cancellationToken)
        {
            IAccountRepository accountRepository = storageService.ForGeneralUseUnitOfWork.GetRepository<IAccountRepository>();

            foreach (DataAccess.Objects.Account account in (await accountRepository.GetAllAsync()).Where(obj => obj.IsMarkedForDeletion))
            {
                await accountRepository.DeleteAsync(account.AccountId);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                //Slow things down
                try
                {
                    await Task.Delay(wait, cancellationToken);
                }
                catch { }
            }
        }
    }
}
