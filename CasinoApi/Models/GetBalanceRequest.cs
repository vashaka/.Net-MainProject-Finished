namespace CasinoApi.Models
{
    public class GetBalanceRequest
    {
        public string Token { get; set; }
        public int? GameId { get; set; }
        public int? ProductId { get; set; }
        public string? Hash { get; set; }
        public string? Currency {  get; set; }
    }
}
