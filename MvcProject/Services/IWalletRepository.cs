namespace MvcProject.Services
{
    public interface IWalletRepository
    {
        public Task CreateWalletAndAssignToUserAsync(string userId, string currency);
    }
}
