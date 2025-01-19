using Dapper;
using Microsoft.Data.SqlClient;
using MvcProject.Repositories;
using System.Configuration;

namespace MvcProject.Services
{

    public class WalletService : IWalletService
    {
        private readonly string _connectionString;
        private readonly ILogger<WalletRepository> _logger;

        public WalletService(IConfiguration configuration, ILogger<WalletRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> ValidateWithdrawAsync(string userId, decimal amount)
        {
            const string balanceSql = @"SELECT CurrentBalance, BlockedAmount FROM Wallets WHERE UserId = @UserId";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);

                var wallet = await connection.QuerySingleOrDefaultAsync<(int CurrentBalance, int BlockedAmount)>(
                    balanceSql, parameters);

                if (wallet == default)
                {
                    return (false, "Wallet not found for the user.");
                }

                if (wallet.BlockedAmount > 0)
                {
                    return (false, "Withdrawals are not allowed as there is a blocked amount.");
                }

                if (amount > wallet.CurrentBalance)
                {
                    return (false, "Insufficient balance.");
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                return (false, "Database error occurred while checking wallet balance.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error: {Message}", ex.Message);
                return (false, "An unexpected error occurred.");
            }

            return (true, string.Empty);
        }



    }
}
