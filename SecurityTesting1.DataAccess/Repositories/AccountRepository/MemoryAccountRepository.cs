using Company.DataAccess;
using Company.Utilities;
using SecurityTesting1.DataAccess.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityTesting1.DataAccess.Repositories.AccountRepository
{
    public class MemoryAccountRepository : IAccountRepository
    {
        //To mimic SQL behaviour and force use of add or update methods, objects returned or added should be clones (different reference)
        private readonly List<DataAccess.Objects.Account> _data = new List<DataAccess.Objects.Account>();
        private IMemoryUnitOfWork _unitOfWork = null!;

        public async Task<DataAccess.Objects.Account?> GetByIdAsync(Guid accountId)
        {
            DataAccess.Objects.Account? account = _data.FirstOrDefault(obj => obj.AccountId == accountId);
            return await Task.FromResult(account);
        }

        public async Task<DataAccess.Objects.Account?> GetByAccountNameAsync(string accountName)
        {
            DataAccess.Objects.Account? account = _data.FirstOrDefault(obj => obj.AccountName.ToUpperInvariant() == accountName.ToUpperInvariant());
            return await Task.FromResult(account);
        }

        public async Task AddAsync(Account account)
        {
            await Task.CompletedTask;
            _data.Add(account.Clone());
        }

        public async Task UpdateAsync(Account account)
        {
            await Task.CompletedTask;
            int index = _data.FindIndex(obj => obj.AccountId == account.AccountId);
            if (index >= 0)
            {
                _data[index] = account.Clone();
            }
        }

        public async Task DeleteAsync(Guid accountId)
        {
            _data.RemoveAll(obj => obj.AccountId == accountId);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await Task.FromResult(_data.Clone());
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork;
            }
            set
            {
                _unitOfWork = (IMemoryUnitOfWork)value;
            }
        }
    }
}
