using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;

        public UserController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
        }

        [HttpGet("get-ballance")]
        [Authorize]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userId =User.FindFirstValue(ClaimTypes.NameIdentifier);
            using(var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT CurrentBalance, Currency FROM Wallets WHERE UserId = @UserId";
                var resp = await connection.QueryAsync(sql, new {UserId = userId});
                Console.WriteLine("balance updates");

                if (resp == null)
                {
                    return NotFound("Wallet not found for the user.");
                }

                return Ok(resp);
            }
        }
    }
}
