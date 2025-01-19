using Dapper;
using Microsoft.Data.SqlClient;
using MvcProject.Models;

namespace MvcProject.Repositories
{
    public class AdminRepository(IConfiguration configuration, ILogger<AdminRepository> logger) : IAdminRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
        private readonly ILogger<AdminRepository> _logger = logger;

        public async Task<IEnumerable<DepositWithdrawRequestDto>> GetAllDepositWithdrawRequestsAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string sql = @"
                        SELECT dwr.*, anu.UserName AS UserName
                        FROM DepositWithdrawRequests dwr
                        INNER JOIN AspNetUsers anu ON dwr.UserId = anu.Id";

                IEnumerable<DepositWithdrawRequestDto> allRequests = await connection.QueryAsync<DepositWithdrawRequestDto>(sql);
                return allRequests;
            }
            catch (SqlException e)
            {
                _logger.LogError("Error occurred: {ErrorMessage}", e.Message);
                throw;
            }
        }
    }
}
