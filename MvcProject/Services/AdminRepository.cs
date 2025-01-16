using Dapper;
using Microsoft.Data.SqlClient;
using MvcProject.Models;

namespace MvcProject.Services
{
    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionString;

        public AdminRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
        }
        public async Task<IEnumerable<DepositWithdrawRequestDto>> GetAllDepositWithdrawRequestsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT dwr.*, anu.UserName AS UserName
                        FROM DepositWithdrawRequests dwr
                        INNER JOIN AspNetUsers anu ON dwr.UserId = anu.Id";

                    IEnumerable<DepositWithdrawRequestDto> allRequests = await connection.QueryAsync<DepositWithdrawRequestDto>(sql);
                    return allRequests;
                }
            }
            catch (SqlException e)
            {
                //Console.Error.WriteLine($"error: {e.Message}");
                throw;
            }
        }
    }
}
