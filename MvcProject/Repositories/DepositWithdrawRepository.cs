using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using MvcProject.Models;
using System.Data;

namespace MvcProject.Repositories
{
    public class DepositWithdrawRepository : IDepositWithdrawRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<DepositWithdrawRepository> _logger;

        public DepositWithdrawRepository(IConfiguration configuration, ILogger<DepositWithdrawRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
            _logger = logger;
        }

        public async Task<DepositWithdrawRequest?> GetRequestByIdAndUserIdAsync(int depositWithdrawId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            const string sql = @"
                SELECT 
                    Id, UserId, TransactionType, Amount, Status, CreatedAt 
                FROM DepositWithdrawRequests 
                WHERE 
                    Id = @Id AND UserId = @UserId;
            ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("Id", depositWithdrawId);
                parameters.Add("UserId", userId);

                var request = await connection.QueryFirstOrDefaultAsync<DepositWithdrawRequest>(sql, parameters);

                return request;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }


        public async Task<int> AddDepositRequestAsync(string userId, decimal amount)
        {
            ArgumentNullException.ThrowIfNull(userId);

            const string sql = @"
                EXEC AddDepositRequest 
                    @UserId, 
                    @Amount,
                    @NewRequestId OUTPUT;
                ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Amount", amount);
                parameters.Add("@NewRequestId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);
                int id = parameters.Get<int>("@NewRequestId");
                return id;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<int> AddWithdrawRequestAsync(string userId, decimal amount)
        {
            const string sql = @"
        EXEC CreateWithdrawRequest 
            @UserId, 
            @Amount, 
            @NewRequestId OUTPUT;
    ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Amount", amount);
                parameters.Add("@NewRequestId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);

                // OUTPUT int
                int newRequestId = parameters.Get<int>("@NewRequestId");
                return newRequestId;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }




        public async Task UpdateWithdrawStatusAsync(int depositWithdrawId, decimal amount, bool fromAdmin)
        {
            // Status is Always Rejected here
            const string sql = @"
                EXEC UpdateWithdrawRequestStatus 
                @RequestId, 
                @Amount,
                @FromAdmin;
            ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("RequestId", depositWithdrawId);
                parameters.Add("Amount", amount);
                parameters.Add("FromAdmin", fromAdmin);

                await connection.ExecuteAsync(sql, parameters);
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }

    }
}
