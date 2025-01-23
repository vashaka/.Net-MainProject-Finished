using Microsoft.AspNetCore.Mvc;
using MvcProject.Helpers;
using MvcProject.Models;
using MvcProject.Repositories;
using System.Text;


public class BankingApiService
{
    private readonly HttpClient _httpClient;
    private readonly IDepositWithdrawRepository _depWithRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ILogger<BankingApiService> _logger;
    private readonly IConfiguration _configuration;

    public BankingApiService(HttpClient httpClient, IDepositWithdrawRepository depWithRepo, ITransactionRepository transactionRepo, ILogger<BankingApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _depWithRepo = depWithRepo;
        _transactionRepo = transactionRepo;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> CallAdminBankingApi(int id, decimal amount, string status, string hash)
    {
        string baseUrl = _configuration["AppSettings:BankingApiBaseUrl"]!;
        var bankingApiUrl = $"{baseUrl}/BankingApi/Withdraw";

        var payload = new
        {
            DepositWithdrawRequestId = id,
            Amount = amount,
            Status = status,
            Hash = hash
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(bankingApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response Body: {message}", responseBody);
            var resp = System.Text.Json.JsonSerializer.Deserialize<BankingApiResponse>(responseBody);
            if(resp != null)
            {
                return resp.Status;
            }
            return "Rejected";
        }
        else
        {
            _logger.LogWarning("Error");
            return "Server-Rejected";
        }
    }

    public async Task<BankingApiResponse> CallDepositBankingApiAsync(decimal amount, string userId)
    {
        int depositWithdrawId = await _depWithRepo.AddDepositRequestAsync(userId, amount);
        string secretKey = _configuration["AppSettings:SecretKey"]!;
        string baseUrl = _configuration["AppSettings:BankingApiBaseUrl"]!;
        string MerchantId = _configuration["AppSettings:MerchantId"]!;

        string hash = HashingHelper.GenerateSHA256Hash(amount.ToString(),depositWithdrawId.ToString(), secretKey, MerchantId);
        var bankingApiUrl = $"{baseUrl}/BankingApi/Deposit";

        var payload = new
        {
            Amount = amount,
            DepositWithdrawRequestId = depositWithdrawId,
            Hash = hash,
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(bankingApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response Body: {message}", responseBody);
            return System.Text.Json.JsonSerializer.Deserialize<BankingApiResponse>(responseBody);
        }
        else
        {
            _logger.LogError("Error");
            throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }
    }

    public async Task CallFinishDepositBankingApiAsync(decimal amount, string userId, int depositWithdrawId, string hash)
    {
        string baseUrl = _configuration["AppSettings:BankingApiBaseUrl"]!;
        var bankingApiUrl = $"{baseUrl}/BankingApi/DepositFinish";

        var payload = new
        {
            Amount = amount,
            DepositWithdrawRequestId = depositWithdrawId,
            Hash = hash,
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(bankingApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var resp = System.Text.Json.JsonSerializer.Deserialize<BankingApiResponse>(responseBody);
            if(resp != null)
            {
                await _transactionRepo.CreateDepositTransactionAsync(userId, resp.Status, amount, depositWithdrawId);
            }
            _logger.LogInformation("Soemthing bad in BankingApi Service: MVC");
        }
        else
        {
            _logger.LogError("Error");
            throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }
    }
}
