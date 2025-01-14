namespace MvcProject.Models
{
    public class DepositWithdrawRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
