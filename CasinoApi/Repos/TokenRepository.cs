
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CasinoApi.Repos
{
    public class TokenRepository : ITokenRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TokenRepository> _logger;

        public TokenRepository(IConfiguration configuration, ILogger<TokenRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
            _logger = logger;
        }
        public async Task<(string, int)> GeneratePrivateTokenAsync(string userId, string publicToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@PublicToken", publicToken);
                parameters.Add("ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("GeneratePrivateToken", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                int returnCode = parameters.Get<int>("ReturnCode");
                string returnMessage = parameters.Get<string>("ReturnMessage") ?? "Unknown error.";

                return (returnMessage, returnCode);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database operation failed in {MethodName}: {Message}", nameof(GeneratePrivateTokenAsync), ex.Message);
                return ("ERROR", 500);
            }
        }

        public async Task<(string?, string, int)> ActivatePrivateTokenAsync(string publicToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PublicToken", publicToken);
                parameters.Add("@PrivateToken", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
                parameters.Add("ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("ActivatePrivateToken", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                int returnCode = parameters.Get<int>("ReturnCode");
                string returnMessage = parameters.Get<string>("ReturnMessage") ?? "Unknown error.";
                if (returnCode != 200)
                {
                    _logger.LogInformation("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (null, returnMessage, returnCode);
                }
                string privateToken = parameters.Get<string>("@PrivateToken");


                return (privateToken, returnMessage, returnCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database operation failed in {MethodName}: {Message}", nameof(ActivatePrivateTokenAsync), ex.Message);
                return (null, "Server Error", 500);
            }
        }
    }
}
