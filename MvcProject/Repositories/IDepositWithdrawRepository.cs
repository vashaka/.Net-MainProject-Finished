using MvcProject.Models;

namespace MvcProject.Repositories
{
    public interface IDepositWithdrawRepository
    {
        Task<int> AddDepositRequestAsync(string userId, decimal amount);
        Task<(int, string)> AddWithdrawRequestAsync(string userId, decimal amount);
        Task UpdateWithdrawStatusAsync(int depositWithdrawId, decimal amount, bool fromAdmin);
        Task<DepositWithdrawRequest?> GetRequestByIdAndUserIdAsync(int depositWithdrawId, string userId);
        Task<DepositWithdrawRequest?> GetRequestByIdAsync(int depositWithdrawId);
    }
}
