using CasinoApi.Models;

namespace CasinoApi.Repos
{
    public interface IActionsRepository
    {

        Task<(decimal, int, string)> MakeBetAsync(string privateToken, decimal amount);
        Task<(decimal, int, string)> WinAsync(string privateToken, decimal amount);
        Task<(decimal, int, string)> CancelBetAsync(string privateToken, int betTransactionId);
        Task<(decimal, int, string)> GetBalanceAsync(string privateToken);
        Task<(UserInfoResponse?, int, string)> GetUserInfoAsync(string privateToken);
    }
}
