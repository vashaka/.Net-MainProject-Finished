using BankingApi.Helpers;
using BankingApi.Models;
using BankingApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BankingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankingApiController : ControllerBase
    {
        private readonly CallingCallBackWithdrawService _callBackService;
        private readonly ILogger<BankingApiController> _logger;
        public BankingApiController(CallingCallBackWithdrawService callBackService, ILogger<BankingApiController> logger)
        {
            _callBackService = callBackService;
            _logger = logger;
        }

        [HttpPost("DepositFinish")]
        public IActionResult DepositFinish([FromBody] BankingApiDepositRequest request)
        {
            // SEND REQUEST TO MVC    CALLBACKCONTROLLER
            const string secretKey = "vashaka_secret_keyy";
            string hash = HashingHelper.GenerateSHA256Hash(
                    request.Amount.ToString(),
                    request.UserId,
                    request.DepositWithdrawRequestId.ToString(),
                    secretKey
            );
            if (hash != request.Hash)
            {
                _logger.LogInformation("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }
            if (request.Amount % 2 == 1)
            {
                _logger.LogInformation("NOT CORRECT, AMOUNT IS EVEN");
                return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = request.DepositWithdrawRequestId });
            }
            return Ok(new { Status = "Success", Hash = hash, depositwithdrawrequestid = request.DepositWithdrawRequestId });
        }



        [HttpPost("Deposit")]
        public IActionResult Deposit([FromBody] BankingApiDepositRequest request)
        {
            const string secretKey = "vashaka_secret_keyy";
            _logger.LogInformation("From BankingApi");
            string hash = HashingHelper.GenerateSHA256Hash(request.Amount.ToString(),request.UserId,request.DepositWithdrawRequestId.ToString(),secretKey);
            if (hash != request.Hash)
            {
                _logger.LogInformation("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }

            return Ok(new { Status = "Success", Hash = hash, depositwithdrawrequestid = request.DepositWithdrawRequestId });
        }

        // calledfromadminContr
        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw(BankingApiAdminRequest adminRequest)
        {
            const string secretKey = "vashaka_secret_keyy";
            string hash = HashingHelper.GenerateSHA256Hash(adminRequest.Amount.ToString(),adminRequest.UserId,adminRequest.DepositWithdrawRequestId.ToString(),secretKey);
            if (hash != adminRequest.Hash)
            {
                _logger.LogInformation("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }
            if (adminRequest.Amount % 2 == 1)
            {
                _logger.LogInformation("NOT CORRECT, AMOUNT IS EVEN");
                string resp1 = await _callBackService.CallCallBackControllerWithdraw(adminRequest.DepositWithdrawRequestId, adminRequest.UserId, "Rejected");
                if (resp1 == "ERROR")
                {
                    return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = adminRequest.DepositWithdrawRequestId });
                }
                return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = adminRequest.DepositWithdrawRequestId });
            }
            string resp = await _callBackService.CallCallBackControllerWithdraw(adminRequest.DepositWithdrawRequestId, adminRequest.UserId, "Success");
            if (resp == "ERROR")
            {
                return Ok(new { Status = "Rejected", Hash = hash, depositwithdrawrequestid = adminRequest.DepositWithdrawRequestId });
            }
            return Ok(new { Status = "Success" });
        }
    }
}
