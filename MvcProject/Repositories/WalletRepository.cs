using Dapper;
using Microsoft.Data.SqlClient;

namespace MvcProject.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly string _connectionString;

        public WalletRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
        }

        public async Task CreateWalletAndAssignToUserAsync(string userId, string currency)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Begin a transaction
            using var transaction = connection.BeginTransaction();
            try
            {
                string sql1 = @"
                    INSERT INTO Wallets (UserId, CurrentBalance, Currency) 
                    VALUES (@UserId, @CurrentBalance, @Currency); 
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int currencyValue = (currency == "GEL") ? 1 : (currency == "USD") ? 2 : (currency == "EUR") ? 3 : 0;

                if (currencyValue == 0)
                {
                    throw new ArgumentException("Invalid currency.");
                }

                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);
                parameters.Add("CurrentBalance", 0.00m);
                parameters.Add("Currency", currencyValue);

                var walletId = await connection.QuerySingleAsync<int>(
                    sql1,
                    parameters,
                    transaction);

                string sql2 = @"
                    UPDATE AspNetUsers 
                    SET WalletId = @WalletId 
                    WHERE Id = @UserId";

                var updateParameters = new DynamicParameters();
                updateParameters.Add("WalletId", walletId);
                updateParameters.Add("UserId", userId);

                await connection.ExecuteAsync(sql2, updateParameters, transaction);

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }



    }
}
