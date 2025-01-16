
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MvcProject.Models;

namespace MvcProject.Services
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
        }
        public async Task CreateDepositTransactionAsync(string userId, string status, decimal amount, int DepositWithdrawRequestId)
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
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(sql, new
                    {
                        userId,
                        amount,
                        status,
                        DepositWithdrawRequestId
                    });
                    Console.WriteLine(userId);
                    Console.WriteLine(status);
                    Console.WriteLine(amount);
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Database operation failed: {ex.Message}");
                throw;
            }
        }

        public async Task<string> CreateWithdrawTransactionAsync(string userId, string status, decimal amount, int DepositWithdrawRequestId)
        {
            Console.WriteLine("withdraw transaction successfuly started!!!!");
            const string sql = @"
                EXEC InsertWithdrawTransaction 
            @UserId, 
            @Amount, 
            @Status, 
            @DepositWithdrawRequestId;
    ";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(sql, new
                    {
                        userId,
                        amount,
                        status,
                        DepositWithdrawRequestId
                    });
                    Console.WriteLine(userId);
                    Console.WriteLine(status);
                    Console.WriteLine(amount);
                    return "OK";
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Database operation failed: {ex.Message}");
                //if (ex.Message.Contains("Insufficient balance"))
                //{
                //    throw new InvalidOperationException("Withdrawal failed due to insufficient balance.", ex);
                //}
                return "ERROR";
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetAllMyTransactions(string userId)
        {
            const string sql = @"SELECT t.*, w.Currency FROM Transactions t INNER JOIN Wallets w ON t.UserId = w.UserId WHERE t.UserId = @UserId";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<TransactionDto> allTransactions = await connection.QueryAsync<TransactionDto>(sql, new {UserId = userId});
                    return allTransactions;
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Database operation failed: {ex.Message}");
                throw;
            }
        }
    }
}
