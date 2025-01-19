using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(IConfiguration config) : ControllerBase
    {
        private readonly string _connectionString = config.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

        [HttpGet("get-ballance")]
        [Authorize]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT CurrentBalance, Currency FROM Wallets WHERE UserId = @UserId";

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            var resp = await connection.QueryAsync(sql, parameters);

            if (resp == null || !resp.Any())
            {
                return NotFound("Wallet not found for the user.");
            }

            return Ok(resp);
        }

    }
}
