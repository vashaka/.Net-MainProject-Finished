using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BankingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankingApiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public BankingApiController(HttpClient http)
        {
            _httpClient = http;
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] BankingApiDepositRequest request)
        {
            const string secretKey = "vashaka_secret_keyy";
            Console.WriteLine("From BankingApi");
            string hash = GenerateSHA256Hash(
                    request.Amount.ToString(),
                    request.UserId,
                    request.DepositWithdrawRequestId.ToString(),
                    secretKey
            );
            if (hash != request.Hash)
            {
                Console.WriteLine("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }

            // SEND REQUEST TO MVC    CALLBACKCONTROLLER
            string status;
            if (request.Amount % 2 == 1)
            {
                Console.WriteLine("NOT CORRECT, AMOUNT IS EVEN");
                status = "Rejected";
                await CallCallBackControllerDeposit(request.DepositWithdrawRequestId, request.UserId, status);
                return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = request.DepositWithdrawRequestId });
            }
            else
            {
                status = "Success";
            }
            await CallCallBackControllerDeposit(request.DepositWithdrawRequestId, request.UserId, status);
            return Ok(new { Status = "Success", Hash = hash, depositwithdrawrequestid = request.DepositWithdrawRequestId });
        }

        // calledfromadminContr
        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw(BankingApiAdminRequest adminRequest)
        {
            const string secretKey = "vashaka_secret_keyy";
            string hash = GenerateSHA256Hash(
                    adminRequest.Amount.ToString(),
                    adminRequest.UserId,
                    adminRequest.DepositWithdrawRequestId.ToString(),
                    secretKey
            );
            if (hash != adminRequest.Hash)
            {
                Console.WriteLine("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }
            string status;
            if (adminRequest.Amount % 2 == 1)
            {
                Console.WriteLine("NOT CORRECT, AMOUNT IS EVEN");
                status = "Rejected";
                string resp1 = await CallCallBackControllerWithdraw(adminRequest.DepositWithdrawRequestId, adminRequest.UserId, status);
                if (resp1 == "ERROR")
                {
                    return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = adminRequest.DepositWithdrawRequestId });
                }
                return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = adminRequest.DepositWithdrawRequestId });
            }
            else
            {
                status = "Success";
            }
            string resp = await CallCallBackControllerWithdraw(adminRequest.DepositWithdrawRequestId, adminRequest.UserId, status);
            if (resp == "ERROR")
            {
                return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = adminRequest.DepositWithdrawRequestId });
            }
            return Ok(new { Status = "Success" });
            //Console.WriteLine("Admin func in bankingApi");
            //Console.WriteLine(adminRequest.DepositWithdrawRequestId);
            //Console.WriteLine(adminRequest.Amount);
            //Console.WriteLine(adminRequest.Status);
            //Console.WriteLine(adminRequest.TransactionType);
            //Console.WriteLine(adminRequest.UserId);
            //return Ok("Hello");
        }

        private async Task CallCallBackControllerDeposit(int depositWithDrawId, string userId, string status)
        {
            Console.WriteLine("BankignApi is calling to CalbackController");
            var bankingApiUrl = "https://localhost:7200/CallBack/Deposit/Confirm";

            var payload = new
            {
                reqid = depositWithDrawId,
                userid = userId,
                status
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(bankingApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody); 
                //return System.Text.Json.JsonSerializer.Deserialize(responseBody);
            }
            else
            {
                Console.WriteLine("error");
                throw new Exception($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }

        private async Task<string> CallCallBackControllerWithdraw(int depositWithDrawId, string userId, string status)
        {
            Console.WriteLine("BankignApi is calling to CalbackController");
            var bankingApiUrl = "https://localhost:7200/CallBack/Withdraw/Confirm";

            var payload = new
            {
                reqid = depositWithDrawId,
                userid = userId,
                status
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(bankingApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody);
                return "OK";
                //return System.Text.Json.JsonSerializer.Deserialize(responseBody);
            }
            else
            {
                Console.WriteLine($"Banking API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return "ERROR";
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
