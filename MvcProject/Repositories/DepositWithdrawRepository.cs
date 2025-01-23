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

        public async Task<DepositWithdrawRequest?> GetRequestByIdAsync(int depositWithdrawId)
        {

            const string sql = @"
                SELECT 
                    Id, UserId, TransactionType, Amount, Status, CreatedAt 
                FROM DepositWithdrawRequests 
                WHERE 
                    Id = @Id;
            ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("Id", depositWithdrawId);

                var request = await connection.QueryFirstOrDefaultAsync<DepositWithdrawRequest>(sql, parameters);

                return request;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw new Exception("Database operation failed. Please try again later.", ex);
            }
        }


        public async Task<int> AddDepositRequestAsync(string userId, decimal amount)
        {
            ArgumentNullException.ThrowIfNull(userId);
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Amount", amount);
                parameters.Add("@NewRequestId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("AddDepositRequest", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                int returnCode = parameters.Get<int>("ReturnCode");
                string returnMessage = parameters.Get<string>("ReturnMessage") ?? "Unknown error.";
                int id = parameters.Get<int?>("@NewRequestId") ?? 0;

                if (returnCode != 0)
                {
                    throw new InvalidOperationException($"Stored procedure error (Code: {returnCode}): {returnMessage}");
                }

                return id;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation failed in {MethodName}: {Message}", nameof(AddDepositRequestAsync), ex.Message);
                throw new Exception("Database operation failed. Please try again later.", ex);
            }
        }


        public async Task<(int, string)> AddWithdrawRequestAsync(string userId, decimal amount)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@Amount", amount);
                parameters.Add("@NewRequestId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("CreateWithdrawRequest", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                int returnCode = parameters.Get<int>("ReturnCode");
                string returnMessage = parameters.Get<string>("ReturnMessage");
                if (returnCode>0)
                {
                    return (0, returnMessage);
                }
                int newRequestId = parameters.Get<int>("@NewRequestId");
                return (newRequestId, returnMessage);
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw new Exception("Error Code: 414, Message: From Server");
            }
        }

        public async Task UpdateWithdrawStatusAsync(int depositWithdrawId, decimal amount, bool fromAdmin)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@depositWithdrawId", depositWithdrawId);
                parameters.Add("@Amount", amount);
                parameters.Add("@FromAdmin", fromAdmin);
                parameters.Add("ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ErrorMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("UpdateWithdrawRequestStatus", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

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
    }
}
