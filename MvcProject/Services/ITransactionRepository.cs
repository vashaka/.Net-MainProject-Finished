using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;

namespace MvcProject.Services
{
    public interface ITransactionRepository
    {
        Task CreateTransactionAsync(string userId, string status, decimal amount, int DepositWithdrawRequestId);
        Task<IEnumerable<Transaction>> GetAllMyTransactions(string userId);
    }
}
