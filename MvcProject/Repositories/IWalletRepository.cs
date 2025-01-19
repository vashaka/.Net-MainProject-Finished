namespace MvcProject.Repositories
{
    public interface IWalletRepository
    {
        public Task CreateWalletAndAssignToUserAsync(string userId, string currency);
    }
}
