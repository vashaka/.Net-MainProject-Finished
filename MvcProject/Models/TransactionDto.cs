﻿namespace MvcProject.Models
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? BetId { get; set; }
        public int? GameId { get; set; }
        public int? RoundId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Currency {  get; set; }
    }
}
