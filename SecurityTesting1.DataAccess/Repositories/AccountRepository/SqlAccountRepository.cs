using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Company.DataAccess;
using Microsoft.Extensions.Logging;
using SecurityTesting1.DataAccess.Databases;
using SecurityTesting1.DataAccess.Objects;

namespace SecurityTesting1.DataAccess.Repositories.AccountRepository
{
    public class SqlAccountRepository : IAccountRepository
    {
        private readonly ILogger _logger;
        private ISqlUnitOfWork _unitOfWork = null!;

        public SqlAccountRepository(ILogger<SqlAccountRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddAsync(Account account)
        {
            await Task.CompletedTask;
            using (Benchmark _ = new(_logger, message: nameof(AccountDatabase.AddAccount), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                AccountDatabase.AddAccount(_unitOfWork.ConnectionString, account);
            }
        }

        public async Task DeleteAsync(Guid accountId)
        {
            await Task.CompletedTask;
            using (Benchmark _ = new(_logger, message: nameof(AccountDatabase.DeleteAccount), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                AccountDatabase.DeleteAccount(_unitOfWork.ConnectionString, accountId);
            }
        }

        public async Task<DataAccess.Objects.Account?> GetByIdAsync(Guid accountId)
        {
            await Task.CompletedTask;
            using (Benchmark _ = new(_logger, message: nameof(AccountDatabase.GetAccountById), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                return AccountDatabase.GetAccountById(_unitOfWork.ConnectionString, accountId);
            }
        }

        public async Task<DataAccess.Objects.Account?> GetByAccountNameAsync(string accountName)
        {
            await Task.CompletedTask;
            using (Benchmark _ = new(_logger, message: nameof(AccountDatabase.GetAccountByAccountName), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                return AccountDatabase.GetAccountByAccountName(_unitOfWork.ConnectionString, accountName);
            }
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            await Task.CompletedTask;
            using (Benchmark _ = new(_logger, message: nameof(AccountDatabase.GetAllAccounts), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                return AccountDatabase.GetAllAccounts(_unitOfWork.ConnectionString);
            }
        }

        public async Task UpdateAsync(Account account)
        {
            await Task.CompletedTask;
            using (Benchmark _ = new(_logger, message: nameof(AccountDatabase.UpdateAccount), thresholdInMilliseconds: _unitOfWork.WarningThresholdInMilliseconds))
            {
                AccountDatabase.UpdateAccount(_unitOfWork.ConnectionString, account);
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
