
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
        public async Task<string> GeneratePrivateTokenAsync(string userId, string publicToken)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(publicToken);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@PublicToken", publicToken);
                parameters.Add("@NewPrivateToken", dbType: DbType.String, size: 50, direction: ParameterDirection.Output); 
                parameters.Add("ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("GeneratePrivateToken", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                int returnCode = parameters.Get<int>("ReturnCode");
                string returnMessage = parameters.Get<string>("ReturnMessage") ?? "Unknown error.";
                string newPrivateToken = parameters.Get<string>("@NewPrivateToken");

                if (returnCode != 0)
                {
                    _logger.LogError("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return "ERROR";
                }

                return newPrivateToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database operation failed in {MethodName}: {Message}", nameof(GeneratePrivateTokenAsync), ex.Message);
                return "ERROR";
            }
        }

        public async Task<string> RetrievePrivateToken(string publicToken)
        {
            string sql = @"SELECT PrivateToken 
                FROM Tokens 
                WHERE PublicToken = @PublicToken 
                AND IsActive = 1";
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@PublicToken", publicToken);
                string? privateToken = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters);
                if(privateToken != null)
                {
                    return privateToken;
                }
                return "ERROR";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database operation failed in {MethodName}: {Message}", nameof(GeneratePrivateTokenAsync), ex.Message);
                return "ERROR";
            }

        }

    }
}
