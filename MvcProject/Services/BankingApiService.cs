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
    private readonly ILogger<BankingApiResponse> _logger;

    public BankingApiService(HttpClient httpClient, IDepositWithdrawRepository depWithRepo, ITransactionRepository transactionRepo, ILogger<BankingApiResponse> logger)
    {
        _httpClient = httpClient;
        _depWithRepo = depWithRepo;
        _transactionRepo = transactionRepo;
        _logger = logger;
    }

    public async Task CallAdminBankingApi(int id, decimal amount, string status, string transactionType, string userId, string hash)
    {
        var bankingApiUrl = "https://localhost:7194/api/BankingApi/Withdraw";

        var payload = new
        {
            DepositWithdrawRequestId = id,
            Amount = amount,
            Status = status,
            TransactionType = transactionType,
            UserId = userId,
            Hash = hash
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(bankingApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response Body: {message}", responseBody);
        }
        else
        {
            _logger.LogWarning("Error");
            throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }
    }

    public async Task<BankingApiResponse> CallDepositBankingApiAsync(decimal amount, string userId)
    {
        // TEST THIS ........
        int depositWithdrawId = await _depWithRepo.AddDepositRequestAsync(userId, amount);
        const string secretKey = "vashaka_secret_keyy";
        string hash = HashingHelper.GenerateSHA256Hash(amount.ToString(), userId, depositWithdrawId.ToString(), secretKey);
        var bankingApiUrl = "https://localhost:7194/api/BankingApi/Deposit";

        var payload = new
        {
            Amount = amount,
            UserId = userId,
            DepositWithdrawRequestId = depositWithdrawId,
            Hash = hash
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
        var bankingApiUrl = "https://localhost:7194/api/BankingApi/DepositFinish";

        var payload = new
        {
            Amount = amount,
            UserId = userId,
            DepositWithdrawRequestId = depositWithdrawId,
            Hash = hash
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
