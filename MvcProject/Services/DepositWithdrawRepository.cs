using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using MvcProject.Models;
using System.Data;

namespace MvcProject.Services
{
    public class DepositWithdrawRepository : IDepositWithdrawRepository
    {
        private readonly string _connectionString;

        public DepositWithdrawRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
        }

        public async Task<DepositWithdrawRequest?> GetRequestByIdAndUserIdAsync(int depositWithdrawId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            const string sql = @"
                SELECT 
                    Id, UserId, TransactionType, Amount, Status, CreatedAt FROM DepositWithdrawRequests 
                    WHERE 
                    Id = @Id AND UserId = @UserId;
    ";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var request = await connection.QueryFirstOrDefaultAsync<DepositWithdrawRequest>(sql, new
                    {
                        Id = depositWithdrawId,
                        UserId = userId
                    });

                    return request;
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Database operation failed: {ex.Message}");
                throw;
            }
        }

        public async Task<int> AddRequestAsync(DepositWithdrawRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            const string sql = @"
                EXEC AddDepositWithdrawRequest 
                    @UserId, 
                    @TransactionType, 
                    @Amount, 
                    @Status, 
                    @CreatedAt;
            ";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var id = await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        request.UserId,
                        request.TransactionType,
                        request.Amount,
                        request.Status,
                        request.CreatedAt
                    });
                    Console.WriteLine(id + "from service");
                    return id;
                }

            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Database operation failed: {ex.Message}");
                throw;
            }
        }



        public async Task UpdateDepositWithdrawStatusAsync(int depositWithdrawId, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException(nameof(status));
            }

            const string sql = @"
                EXEC UpdateDepositWithdrawRequestStatus 
                @RequestId, 
                @Status;
            ";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(sql, new
                    {
                        RequestId = depositWithdrawId,
                        Status = status
                    });
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
