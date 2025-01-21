using System.Text.Json.Serialization;

namespace MvcProject.Models
{
    public class BankingApiResponse
    {
        [JsonPropertyName("status")]
        public string Status {  get; set; }
        [JsonPropertyName("hash")]
        public int DepositWithdrawRequestId { get; set; }
        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }
    }
}
