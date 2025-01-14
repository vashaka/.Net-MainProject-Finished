namespace BankingApi.Models
{
    public class BankingApiDepositRequest
    {
        public int DepositWithdrawRequestId { get; set; }
        public decimal Amount { get; set; }
        public string UserId { get; set; }
        public string Hash { get; set; }
    }
}
