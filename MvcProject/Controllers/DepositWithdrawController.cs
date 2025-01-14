using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    [Authorize]
    public class DepositWithdrawController : Controller
    {
        private readonly IDepositWithdrawRepository _depWithRepo;
        private readonly HttpClient _httpClient;
        public DepositWithdrawController(IDepositWithdrawRepository depWithRepo, HttpClient http)
        {
            _depWithRepo = depWithRepo;
            _httpClient = http;
        }

        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDeposit(decimal amount)
        {
            var amountInCents = amount * 100;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not logged in.");
            }
            var req = new DepositWithdrawRequest
            {
                UserId = userId,
                TransactionType = "Deposit",
                Amount = amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                int depositWithdrawId = await _depWithRepo.AddRequestAsync(req);
                const string secretKey = "vashaka_secret_keyy";
                string hash = GenerateSHA256Hash(
                    amountInCents.ToString(),
                    userId,
                    depositWithdrawId.ToString(),
                    secretKey
                );
                decimal amountInCents1 = amountInCents;
                // sending to bankingApi
                var bankingApiResponse = await CallDepositBankingAPi(amountInCents, userId, depositWithdrawId, hash);
                if (bankingApiResponse.Status == "Rejected")
                {
                    Console.WriteLine("changong DepositWithdrawRequest's status!!");
                    await _depWithRepo.UpdateDepositWithdrawStatusAsync(depositWithdrawId, bankingApiResponse.Status);
                    return Ok();
                }

                Console.WriteLine("DepositWithdrawRequest should not be changed!!");
                string redirectUrl = $"https://localhost:7200/CallBack/{bankingApiResponse.Hash}/{bankingApiResponse.DepositWithdrawRequestId}";
                return Ok(new { redirectUrl });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing Deposit: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitWithdraw(decimal amount)
        {
            var amountInCents = amount * 100;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not logged in.");
            }
            var req = new DepositWithdrawRequest
            {
                UserId = userId,
                TransactionType = "Withdraw",
                Amount = amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                int depositWithdrawId = await _depWithRepo.AddRequestAsync(req);
                const string secretKey = "vashaka_secret_keyy";
                string hash = GenerateSHA256Hash(
                    amountInCents.ToString(),
                    userId,
                    depositWithdrawId.ToString(),
                    secretKey
                );
                decimal amountInCents1 = amountInCents;
                // NO SENDING TO BANKING API UNTIL ADMIN APPROVES OR REJECTS
                // sending to bankingApi
                //var bankingApiResponse = await CallWithdrawBankingApi(amountInCents, userId, depositWithdrawId, hash);
                //if(bankingApiResponse.Status == "Rejected")
                //{
                //    Console.WriteLine("changong DepositWithdrawRequest's status!!");
                //    await _depWithService.UpdateDepositWithdrawStatusAsync(depositWithdrawId, bankingApiResponse.Status);
                //} else
                //{
                //    Console.WriteLine("DepositWithdrawRequest should not be changed!!");
                //}

                return RedirectToAction("Deposit");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing withdrawal: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
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

        private async Task<BankingApiResponse> CallWithdrawBankingApi(decimal amount, string userId, int depositWithDrawId, string hash)
        {
            var bankingApiUrl = "https://localhost:7194/api/BankingApi";

            var payload = new
            {
                Amount = amount,
                UserId = userId,
                DepositWithdrawRequestId = depositWithDrawId,
                Hash = hash
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(bankingApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody); // Log response 
                return System.Text.Json.JsonSerializer.Deserialize<BankingApiResponse>(responseBody);
            }
            else
            {
                throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }

        private async Task<BankingApiResponse> CallDepositBankingAPi(decimal amount, string userId, int depositWithDrawId, string hash)
        {
            var bankingApiUrl = "https://localhost:7194/api/BankingApi/Deposit";

            var payload = new
            {
                Amount = amount,
                UserId = userId,
                DepositWithdrawRequestId = depositWithDrawId,
                Hash = hash
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(bankingApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody); // Log response 
                return System.Text.Json.JsonSerializer.Deserialize<BankingApiResponse>(responseBody);
            }
            else
            {
                Console.WriteLine("error");
                throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
