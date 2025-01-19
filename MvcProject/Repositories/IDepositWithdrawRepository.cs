using MvcProject.Models;

namespace MvcProject.Repositories
{
    public interface IDepositWithdrawRepository
    {
        Task<int> AddDepositRequestAsync(string userId, decimal amount);
        Task<int> AddWithdrawRequestAsync(string userId, decimal amount);
        Task UpdateWithdrawStatusAsync(int depositWithdrawId, decimal amount, bool fromAdmin);
        Task<DepositWithdrawRequest?> GetRequestByIdAndUserIdAsync(int depositWithdrawId, string userId);
    }
}
