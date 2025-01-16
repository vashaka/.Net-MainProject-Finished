using Dapper;
using Microsoft.Data.SqlClient;

namespace MvcProject.Services
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Iwyeba Transaction rata orive shesruldes
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql1 =@"INSERT INTO Wallets (UserId, CurrentBalance, Currency) VALUES (@UserId, @CurrentBalance, @Currency); 
                                           SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        int currencyValue=(currency == "GEL") ? 1 : (currency == "USD") ? 2 : (currency == "EUR") ? 3 : 0;

                        if (currencyValue == 0)
                        {
                            throw new ArgumentException("Invalid currency.");
                        }
                        var walletId = await connection.QuerySingleAsync<int>(
                            sql1,
                            new { UserId = userId, CurrentBalance = 0.00m, Currency = currencyValue },
                            transaction);

                        string sql2= @"UPDATE AspNetUsers SET WalletId = @WalletId WHERE Id = @UserId";
                        await connection.ExecuteAsync(sql2, new{ WalletId = walletId, UserId = userId }, transaction);
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


    }
}
