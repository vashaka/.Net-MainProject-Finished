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
        [HttpPost]
        public IActionResult ProcessDepositTransaction([FromBody] BankingApiDepositRequest request)
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

            if (request.Amount % 2 == 1)
            {
                Console.WriteLine("NOT CORRECT, AMOUNT IS EVEN");
                return Ok(new { Status = "Rejected" });
            }
            return Ok(new {Status = "Pending"});
        }

        [HttpPost("Deposit")]
        public IActionResult Deposit([FromBody] BankingApiDepositRequest request)
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

            if (request.Amount % 2 == 1)
            {
                Console.WriteLine("NOT CORRECT, AMOUNT IS EVEN");
                return Ok(new { Status = "Rejected" });
            }
            Console.WriteLine(request.DepositWithdrawRequestId);
            return Ok(new { Status = "Pending", Hash = hash, depositwithdrawrequestid = request.DepositWithdrawRequestId });
        }

        [HttpPost("AdminRequest")]
        public IActionResult AdminRequest(BankingApiAdminRequest adminRequest)
        {
            Console.WriteLine("Admin func in bankingApi");
            Console.WriteLine(adminRequest.DepositWithdrawRequestId);
            Console.WriteLine(adminRequest.Amount);
            Console.WriteLine(adminRequest.Status);
            Console.WriteLine(adminRequest.TransactionType);
            Console.WriteLine(adminRequest.UserId);
            return Ok("Hello");
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
