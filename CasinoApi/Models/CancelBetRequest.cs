namespace CasinoApi.Models
{
    public class CancelBetRequest
    {
        public string Token { get; set; }
        public int Amount { get; set; }
        public string? TransactionId { get; set; }
        public int? BetTypeId { get; set; }
        public int? GameId { get; set; }
        public int? ProductId { get; set; }
        public int? RoundId { get; set; }
        public string? Hash { get; set; }
        public string? Currency { get; set; }
        public int BetTransactionId { get; set; }
    }
}
