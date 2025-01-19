namespace MvcProject.Services
{
    public interface IWalletService
    {
        Task<(bool Success, string Message)> ValidateWithdrawAsync(string userId, decimal amount);
    }
}
