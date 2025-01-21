using System.Net.Http;
using System.Text;

namespace BankingApi.Services
{
    public class CallingCallBackWithdrawService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CallingCallBackWithdrawService> _logger;
        private readonly IConfiguration _configuration;
        public CallingCallBackWithdrawService(HttpClient httpClient, ILogger<CallingCallBackWithdrawService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string> CallCallBackControllerWithdraw(int depositWithDrawId, string status)
        {
            _logger.LogInformation("BankignApi is calling to CalbackController");
            string MvcBaseUrl = _configuration["AppSettings:MvcBaseUrl"]!;
            var bankingApiUrl = $"{MvcBaseUrl}/CallBack/Withdraw/Confirm";

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
