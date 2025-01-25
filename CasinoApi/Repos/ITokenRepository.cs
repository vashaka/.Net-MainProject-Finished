namespace CasinoApi.Repos
{
    public interface ITokenRepository
    {
        Task<string> GeneratePrivateTokenAsync(string userId, string publicToken);
        Task<string> RetrievePrivateToken(string publicToken);
    }
}
