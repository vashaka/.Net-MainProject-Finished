using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;

namespace MvcProject.Repositories
{
    public interface ITransactionRepository
    {
        Task CreateDepositTransactionAsync(string userId, string status, decimal amount, int DepositWithdrawRequestId);
        Task<string> CreateWithdrawTransactionAsync(string userId, string status, decimal amount, int DepositWithdrawRequestId);
        Task<IEnumerable<TransactionDto>> GetAllMyTransactions(string userId);
    }
}
