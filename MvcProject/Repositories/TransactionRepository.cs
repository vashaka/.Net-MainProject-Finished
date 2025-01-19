
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MvcProject.Models;

namespace MvcProject.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TransactionRepository> _logger;

        public TransactionRepository(IConfiguration configuration, ILogger<TransactionRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
            _logger = logger;
        }
        public async Task CreateDepositTransactionAsync(string userId, string status, decimal amount, int depositWithdrawRequestId)
        {
            const string sql = @"
        EXEC InsertDepositTransaction 
            @UserId, 
            @Amount, 
            @Status, 
            @DepositWithdrawRequestId;
    ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);
                parameters.Add("Amount", amount);
                parameters.Add("Status", status);
                parameters.Add("DepositWithdrawRequestId", depositWithdrawRequestId);

                await connection.ExecuteAsync(sql, parameters);
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }


        public async Task<string> CreateWithdrawTransactionAsync(string userId, string status, decimal amount, int depositWithdrawRequestId)
        {
            _logger.LogInformation("Withdraw transaction successfully started!!!!");

            const string sql = @"
        EXEC InsertWithdrawTransaction 
            @UserId, 
            @Amount, 
            @Status, 
            @DepositWithdrawRequestId;
    ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);
                parameters.Add("Amount", amount);
                parameters.Add("Status", status);
                parameters.Add("DepositWithdrawRequestId", depositWithdrawRequestId);

                await connection.ExecuteAsync(sql, parameters);
                return "OK";
            }
            catch (SqlException ex)
            {
                _logger.LogError("Error occurred: {Message}", ex.Message);
                return "ERROR";
            }
        }


        public async Task<IEnumerable<TransactionDto>> GetAllMyTransactions(string userId)
        {
            const string sql = @"
                SELECT t.*, w.Currency 
                FROM Transactions t 
                INNER JOIN Wallets w ON t.UserId = w.UserId 
                WHERE t.UserId = @UserId";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);

                IEnumerable<TransactionDto> allTransactions = await connection.QueryAsync<TransactionDto>(sql, parameters);
                return allTransactions;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }

    }
}
