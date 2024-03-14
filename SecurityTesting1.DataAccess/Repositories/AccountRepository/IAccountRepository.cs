using Company.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SecurityTesting1.DataAccess.Repositories.AccountRepository
{
    public interface IAccountRepository : IRepository
    {
        Task<DataAccess.Objects.Account?> GetByIdAsync(Guid accountId);

        Task<DataAccess.Objects.Account?> GetByAccountNameAsync(string accountName);

        Task<IEnumerable<DataAccess.Objects.Account>> GetAllAsync();

        Task AddAsync(DataAccess.Objects.Account account);

        Task UpdateAsync(DataAccess.Objects.Account account);

        Task DeleteAsync(Guid accountId);
    }
}
