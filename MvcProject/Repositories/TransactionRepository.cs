
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MvcProject.Models;
using System.Data;

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
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Amount", amount);
                parameters.Add("@Status", status);
                parameters.Add("@DepositWithdrawRequestId", depositWithdrawRequestId);
                parameters.Add("ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ErrorMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("InsertDepositTransaction", parameters, commandType: CommandType.StoredProcedure);
                int errorCode = parameters.Get<int>("ErrorCode");
                string errorMessage = parameters.Get<string>("ErrorMessage");

                if (errorCode != 0)
                {
                    _logger.LogError("Database operation failed: ErrorCode {ErrorCode}, Message: {ErrorMessage}", errorCode, errorMessage);
                    throw new Exception($"Error Code: {errorCode}, Message: {errorMessage}");
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }



        public async Task<(int, string)> CreateWithdrawTransactionAsync(string userId, string status, decimal amount, int depositWithdrawRequestId)
        {
            _logger.LogInformation("Withdraw transaction successfully started!!!!");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Amount", amount);
                parameters.Add("@Status", status);
                parameters.Add("@DepositWithdrawRequestId", depositWithdrawRequestId);
                parameters.Add("ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ErrorMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("InsertWithdrawTransaction", parameters, commandType: CommandType.StoredProcedure);

                int errorCode = parameters.Get<int>("ErrorCode");
                string errorMessage = parameters.Get<string>("ErrorMessage");

                //if (errorCode != 0)
                //{
                //    _logger.LogError("Error occurred: {ErrorCode}, Message: {ErrorMessage}", errorCode, errorMessage);
                //    return $"ERROR: {errorMessage}";
                //}

                if (errorCode > 0)
                {
                    return (errorCode, errorMessage);
                }

                return (0, "OK");
            }
            catch (SqlException ex)
            {
                _logger.LogError("Error occurred: {Message}", ex.Message);
                return (1, "ERROR");
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
