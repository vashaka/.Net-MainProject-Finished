using System.Net.Http;
using System.Text;

namespace BankingApi.Services
{
    public class CallingCallBackWithdrawService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CallingCallBackWithdrawService> _logger;
        public CallingCallBackWithdrawService(HttpClient httpClient, ILogger<CallingCallBackWithdrawService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<string> CallCallBackControllerWithdraw(int depositWithDrawId, string status)
        {
            _logger.LogInformation("BankignApi is calling to CalbackController");
            var bankingApiUrl = "https://localhost:7200/CallBack/Withdraw/Confirm";

            var payload = new
            {
                RequestId = depositWithDrawId,
                Status = status
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(bankingApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response Body: {Message}", responseBody);
                return "OK";
                //return System.Text.Json.JsonSerializer.Deserialize(responseBody);
            }
            else
            {
                _logger.LogInformation($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return "ERROR";
            }
        }
    }
}
