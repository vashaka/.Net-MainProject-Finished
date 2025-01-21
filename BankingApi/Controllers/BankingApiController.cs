using BankingApi.Helpers;
using BankingApi.Models;
using BankingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankingApiController : ControllerBase
    {
        private readonly CallingCallBackWithdrawService _callBackService;
        private readonly ILogger<BankingApiController> _logger;
        private readonly IConfiguration _configuration;
        public BankingApiController(CallingCallBackWithdrawService callBackService, ILogger<BankingApiController> logger, IConfiguration configuration)
        {
            _callBackService = callBackService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("DepositFinish")]
        public IActionResult DepositFinish([FromBody] BankingApiDepositRequest request)
        {
            string secretKey = _configuration["AppSettings:SecretKey"]!;
            string MerchantId = _configuration["AppSettings:MerchantId"]!;
            string hash = HashingHelper.GenerateSHA256Hash(request.Amount.ToString(),request.DepositWithdrawRequestId.ToString(),secretKey, MerchantId);
            if (hash != request.Hash)
            {
                _logger.LogInformation("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }
            string status;
            if (request.Amount % 2 == 1)
            {
                _logger.LogInformation("NOT CORRECT, AMOUNT IS EVEN");
                status = "Rejected";
            }
            else
            {
                status = "Success";
            }
            return Ok(new { Status = status, depositwithdrawrequestid = request.DepositWithdrawRequestId });
        }



        [HttpPost("Deposit")]
        public IActionResult Deposit([FromBody] BankingApiDepositRequest request)
        {
            string secretKey = _configuration["AppSettings:SecretKey"]!;
            string MvcBaseUrl = _configuration["AppSettings:MvcBaseUrl"]!;
            string MerchantId = _configuration["AppSettings:MerchantId"]!;
            string hash = HashingHelper.GenerateSHA256Hash(request.Amount.ToString(),request.DepositWithdrawRequestId.ToString(),secretKey, MerchantId);
            if (hash != request.Hash)
            {
                _logger.LogInformation("INCORRECT HASHING, !!!! HERE HERE HERE");
                return Ok(new { Status = "Rejected" });
            }

            string redirectUrl = $"{MvcBaseUrl}/CallBack/{request.DepositWithdrawRequestId}";
            return Ok(new { Status = "Success", RedirectUrl = redirectUrl });
        }

        // calledfromadminContr
        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw(BankingApiAdminRequest adminRequest)
        {
            string secretKey = _configuration["AppSettings:SecretKey"]!;
            string MerchantId = _configuration["AppSettings:MerchantId"]!;
            string hash = HashingHelper.GenerateSHA256Hash(adminRequest.Amount.ToString(),adminRequest.DepositWithdrawRequestId.ToString(),secretKey, MerchantId);

            if (hash != adminRequest.Hash)
            {
                _logger.LogInformation("INCORRECT HASHING, !!!!");
                return Ok(new { Status = "Rejected" });
            }

            string status;
            if (adminRequest.Amount % 2 == 1)
            {
                _logger.LogInformation("NOT CORRECT, AMOUNT IS EVEN");
                status = "Rejected";
            }
            else
            {
                status = "Success";
            }
            string resp = await _callBackService.CallCallBackControllerWithdraw(adminRequest.DepositWithdrawRequestId, status);
            return Ok(new { Status = resp == "ERROR" ? "Rejected" : status });
        }
    }
}
