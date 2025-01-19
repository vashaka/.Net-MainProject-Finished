namespace MvcProject.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal BlockedAmount { get; set; }
        public int Currency { get; set; } // 1 = EUR, 2 = USD, 3 = GEL
    }
}
