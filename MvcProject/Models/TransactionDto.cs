namespace MvcProject.Models
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Currency {  get; set; }
    }
}
