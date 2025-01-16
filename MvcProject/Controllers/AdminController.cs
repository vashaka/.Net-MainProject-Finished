using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using System.Linq;


namespace MvcProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly HttpClient _httpClient;
        private readonly IDepositWithdrawRepository _depositWithdrawRepo;

        public AdminController(IAdminRepository adminRepo, HttpClient http, IDepositWithdrawRepository depositWithdrawRepo)
        {
            _httpClient = http;
            _adminRepo = adminRepo;
            _depositWithdrawRepo = depositWithdrawRepo;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<DepositWithdrawRequestDto> getAllRequests = await _adminRepo.GetAllDepositWithdrawRequestsAsync();
            IEnumerable<DepositWithdrawRequestDto> sorted = getAllRequests.Where(r => r.TransactionType == "Withdraw").OrderByDescending(r => r.CreatedAt).ToList();

            return View(sorted);
        }

        [HttpPost]
        public async Task<IActionResult> AdminApproveReject(int id, decimal amount, string status, string transactionType, string userId)
        {
            if (status == "Rejected")
            {
                await _depositWithdrawRepo.UpdateDepositWithdrawStatusAsync(id, status);
                return Ok();
            }
            const string secretKey = "vashaka_secret_keyy";
            Console.WriteLine("From BankingApi");
            string hash = GenerateSHA256Hash(
                    amount.ToString(),
                    userId,
                    id.ToString(),
                    secretKey
            );


            Console.WriteLine("HEYYEYEYEYEYEYE");
            Console.WriteLine($"Id: {id}, Amount: {amount}, Status: {status}, TransactionType: {transactionType}, UserId: {userId}");
            await CallAdminBankingApi(id, amount, status, transactionType, userId, hash);
            return Ok();
        }


         private async Task CallAdminBankingApi(int id, decimal amount, string status, string transactionType, string userId, string hash)
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
                 Console.WriteLine("Response Body: " + responseBody); // Log response 
             }
             else
             {
                 throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
             }
         }

        public static string GenerateSHA256Hash(string amount, string userId, string transactionId, string secretKey)
        {
            var concatenatedString = string.Join("+", new[] { amount, userId, transactionId, secretKey });
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
