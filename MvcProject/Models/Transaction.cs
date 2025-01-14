namespace MvcProject.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
