﻿namespace BankingApi.Models
{
    public class BankingApiAdminRequest
    {
        public int DepositWithdrawRequestId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Hash { get; set; }
    }
}
