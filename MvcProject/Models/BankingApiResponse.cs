using System.Text.Json.Serialization;

namespace MvcProject.Models
{
    public class BankingApiResponse
    {
        [JsonPropertyName("status")]
        public string Status {  get; set; }
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
        [JsonPropertyName("depositwithdrawrequestid")]
        public int DepositWithdrawRequestId { get; set; }
    }
}
