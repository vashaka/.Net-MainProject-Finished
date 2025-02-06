
using CasinoApi.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CasinoApi.Repos
{
    public class ActionsRepository : IActionsRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ActionsRepository> _logger;

        public ActionsRepository(IConfiguration configuration, ILogger<ActionsRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
            _logger = logger;
        }

        //        currentBalance, statusCode, ReturnMessage
        public async Task<(decimal, int, string)> MakeBetAsync(string privateToken, decimal amount, string transactionId, int GameId, int RoundId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", privateToken);
                parameters.Add("@Amount", amount);
                parameters.Add("@TransactionId", transactionId);
                parameters.Add("@GameId", GameId);
                parameters.Add("@RoundId", RoundId);

                parameters.Add("@UpdatedBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("CreateBet", parameters, commandType: CommandType.StoredProcedure);

                int returnCode = parameters.Get<int>("@ReturnCode");
                string returnMessage = parameters.Get<string>("@ReturnMessage");
                decimal updatedBalance = parameters.Get<decimal>("@UpdatedBalance");

                if (returnCode != 200)
                {
                    _logger.LogWarning("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (0, returnCode, returnMessage);
                }

                _logger.LogInformation("Bet successful. Updated balance: {UpdatedBalance}", updatedBalance);
                return (updatedBalance, 200, returnMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bad error in: {Message}", ex.Message);
                return (0, 500, "Server Error");
            }
        }

        public async Task<(decimal, int, string)> WinAsync(string privateToken, decimal amount, string transactionId, int GameId, int RoundId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", privateToken);
                parameters.Add("@Amount", amount);
                parameters.Add("@TransactionId", transactionId);
                parameters.Add("@GameId", GameId);
                parameters.Add("@RoundId", RoundId);

                parameters.Add("@UpdatedBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("CreateWin", parameters, commandType: CommandType.StoredProcedure);

                int returnCode = parameters.Get<int>("@ReturnCode");
                string returnMessage = parameters.Get<string>("@ReturnMessage");
                decimal updatedBalance = parameters.Get<decimal>("@UpdatedBalance");

                if (returnCode != 200)
                {
                    _logger.LogWarning("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (0, returnCode, returnMessage);
                }

                _logger.LogInformation("Win Transaction Created Successfuly. Updated balance: {UpdatedBalance}", updatedBalance);
                return (updatedBalance, 200, returnMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bad error in: {Message}", ex.Message);
                return (0, 500, "Server Error");
            }
        }

        public async Task<(decimal, int, string)> CancelBetAsync(string privateToken, string transactionId, string betTransactionId, decimal amount, int GameId, int RoundId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", privateToken);
                parameters.Add("@Amount", amount);
                parameters.Add("@TransactionId", transactionId);
                parameters.Add("@GameId", GameId);
                parameters.Add("@RoundId", RoundId);
                parameters.Add("@BetTransactionId", betTransactionId);

                parameters.Add("@UpdatedBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("CancelBet", parameters, commandType: CommandType.StoredProcedure);

                int returnCode = parameters.Get<int>("@ReturnCode");
                string returnMessage = parameters.Get<string>("@ReturnMessage");
                if (returnCode != 200)
                {
                    _logger.LogWarning("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (0, returnCode, returnMessage);
                }
                decimal updatedBalance = parameters.Get<decimal>("@UpdatedBalance");


                _logger.LogInformation("Bet Canceled successfuly. Updated balance: {UpdatedBalance}", updatedBalance);
                return (updatedBalance, 200, returnMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bad error in: {Message}", ex.Message);
                return (0, 500, "Server Error");
            }
        }

        public async Task<(decimal, int, string)> ChangeWinAsync(string privateToken, string transactionId, string previousTransactionId, decimal amount, decimal previousAmount, int GameId, int RoundId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", privateToken);
                parameters.Add("@Amount", amount);
                parameters.Add("@PrevAmount", previousAmount);
                parameters.Add("@TransactionId", transactionId);
                parameters.Add("@PrevTransactionId", previousTransactionId);
                parameters.Add("@GameId", GameId);
                parameters.Add("@RoundId", RoundId);

                parameters.Add("@UpdatedBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("ChangeWin", parameters, commandType: CommandType.StoredProcedure);

                int returnCode = parameters.Get<int>("@ReturnCode");
                string returnMessage = parameters.Get<string>("@ReturnMessage");
                if (returnCode != 200)
                {
                    _logger.LogWarning("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (0, returnCode, returnMessage);
                }
                decimal updatedBalance = parameters.Get<decimal>("@UpdatedBalance");

                _logger.LogInformation("win Changed successfuly. Updated balance: {UpdatedBalance}", updatedBalance);
                return (updatedBalance, 200, returnMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bad error in: {Message}", ex.Message);
                return (0, 500, "Server Error");
            }
        }

        public async Task<(decimal, int, string)> GetBalanceAsync(string privateToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", privateToken);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("GetBalance", parameters, commandType: CommandType.StoredProcedure);

                int returnCode = parameters.Get<int>("@ReturnCode");
                string returnMessage = parameters.Get<string>("@ReturnMessage");
                decimal currentBalance = parameters.Get<decimal>("@CurrentBalance");

                if (returnCode != 200)
                {
                    _logger.LogWarning("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (0, returnCode, returnMessage);
                }

                _logger.LogInformation("Player's Balance Retrieved Successfully. Updated balance: {CurrentBalance}", currentBalance);
                return (currentBalance, 200, returnMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bad error in: {Message}", ex.Message);
                return (0, 500, "Server Error");
            }
        }

        public async Task<(UserInfoResponse?, int, string)> GetUserInfoAsync(string privateToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", privateToken, dbType: DbType.String, size: 100);
                parameters.Add("@UserId", dbType: DbType.String, direction: ParameterDirection.Output, size: 450);
                parameters.Add("@UserName", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                parameters.Add("@Email", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@Currency", dbType: DbType.Int32, direction: ParameterDirection.Output);

                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ReturnMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("GetUserInfo", parameters, commandType: CommandType.StoredProcedure);

                int returnCode = parameters.Get<int>("@ReturnCode");
                string returnMessage = parameters.Get<string>("@ReturnMessage");

                string? userId = parameters.Get<string?>("@UserId") ?? "";
                string? userName = parameters.Get<string?>("@UserName") ?? "";
                string? email = parameters.Get<string?>("@Email") ?? "";
                decimal? currentBalance = parameters.Get<decimal?>("@CurrentBalance") ?? 0;
                int? currency = parameters.Get<int?>("@Currency") ?? 0;

                if (returnCode != 200)
                {
                    _logger.LogWarning("Stored procedure failed with code {ReturnCode}: {ReturnMessage}", returnCode, returnMessage);
                    return (null, returnCode, returnMessage);
                }

                UserInfoResponse? userInfo = new()
                { 
                    Id = userId, UserName = userName, Email = email, CurrentBalance = (decimal)currentBalance, Currency = (int)currency
                };

                _logger.LogInformation("Player's Info Retrieved Successfully. CurrentBalance balance: {CurrentBalance}", currentBalance);
                return (userInfo, 200, returnMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bad error in: {Message}", ex.Message);
                return (null, 500, "Server Error");
            }
        }
    }
}
