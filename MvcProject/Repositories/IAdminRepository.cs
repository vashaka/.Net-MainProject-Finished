using MvcProject.Models;

namespace MvcProject.Repositories
{
    public interface IAdminRepository
    {
        Task<IEnumerable<DepositWithdrawRequestDto>> GetAllDepositWithdrawRequestsAsync();
    }
}
