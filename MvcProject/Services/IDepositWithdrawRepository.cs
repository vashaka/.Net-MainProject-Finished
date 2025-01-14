using MvcProject.Models;

namespace MvcProject.Services
{
    public interface IDepositWithdrawRepository
    {
        Task<int> AddRequestAsync(DepositWithdrawRequest request);
        Task UpdateDepositWithdrawStatusAsync(int depositWithdrawId, string status);
        Task<DepositWithdrawRequest?> GetRequestByIdAndUserIdAsync(int depositWithdrawId, string userId);
    }
}
