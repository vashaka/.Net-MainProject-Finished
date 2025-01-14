
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
        public async Task CreateTransactionAsync(string userId, string status, decimal amount, int DepositWithdrawRequestId)
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

        public async Task<IEnumerable<Transaction>> GetAllMyTransactions(string userId)
        {
            const string sql = @"SELECT * FROM Transactions WHERE UserId = @UserId";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<Transaction> allTransactions = await connection.QueryAsync<Transaction>(sql, new {UserId = userId});
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
