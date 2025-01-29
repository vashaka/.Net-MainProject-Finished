namespace CasinoApi.Repos
{
    public interface ITokenRepository
    {
        Task<(string, int)> GeneratePrivateTokenAsync(string userId, string publicToken);
        Task<(string?, string, int)> ActivatePrivateTokenAsync(string publicToken);
    }
}
