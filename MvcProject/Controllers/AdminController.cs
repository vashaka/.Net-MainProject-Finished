using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services;
using System.Net.Http;
using System.Text;

namespace MvcProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly HttpClient _httpClient;

        public AdminController(IAdminRepository adminRepo, HttpClient http)
        {
            _httpClient = http;
            _adminRepo = adminRepo;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<DepositWithdrawRequest> getAllRequests = await _adminRepo.GetAllDepositWithdrawRequestsAsync();
            return View(getAllRequests);
        }

        [HttpPost]
        public async Task<IActionResult> AdminApproveReject(int id, decimal amount, string status, string transactionType, string userId)
        {
            Console.WriteLine("HEYYEYEYEYEYEYE");
            Console.WriteLine($"Id: {id}, Amount: {amount}, Status: {status}, TransactionType: {transactionType}, UserId: {userId}");
            await CallAdminBankingApi(id, amount, status, transactionType, userId);
            return Ok();
        }


        private async Task CallAdminBankingApi(int id, decimal amount, string status, string transactionType, string userId)
         {
             var bankingApiUrl = "https://localhost:7194/api/BankingApi/AdminRequest";

             var payload = new
             {
                 DepositWithdrawRequestId = id,
                 Amount = amount,
                 Status = status,
                 TransactionType = transactionType,
                 UserId = userId,
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
    }
}
