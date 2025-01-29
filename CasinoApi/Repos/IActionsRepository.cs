using CasinoApi.Models;

namespace CasinoApi.Repos
{
    public interface IActionsRepository
    {

        Task<(decimal, int, string)> MakeBetAsync(string privateToken, decimal amount, string transactionId, int GameId, int RoundId);
        Task<(decimal, int, string)> WinAsync(string privateToken, decimal amount, string transactionId, int GameId, int RoundId);
        Task<(decimal, int, string)> CancelBetAsync(string privateToken, string transactionId, string betTransactionId, decimal amount, int GameId, int RoundId);
        Task<(decimal, int, string)> ChangeWinAsync(string privateToken, string transactionId,string previousTransactionId, decimal amount, decimal previousAmount, int GameId, int RoundId);
        Task<(decimal, int, string)> GetBalanceAsync(string privateToken);
        Task<(UserInfoResponse?, int, string)> GetUserInfoAsync(string privateToken);
    }
}
