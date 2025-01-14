using MvcProject.Models;

namespace MvcProject.Services
{
    public interface IAdminRepository
    {
        Task<IEnumerable<DepositWithdrawRequest>> GetAllDepositWithdrawRequestsAsync();
    }
}
