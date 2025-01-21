using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Repositories;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using MvcProject.Helpers;

namespace MvcProject.Controllers
{
    //[Authorize]
    public class CallBackController : Controller
    {
        private readonly IDepositWithdrawRepository _depWithRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly BankingApiService _bankingApiService;
        private readonly ILogger<CallBackController> _logger;
        private readonly IConfiguration _configuration;

        public CallBackController(IDepositWithdrawRepository depWithRepo, ITransactionRepository transactionRepo, BankingApiService bankingApiService, ILogger<CallBackController> logger, IConfiguration configuration)
        {
            _depWithRepo = depWithRepo;
            _transactionRepo = transactionRepo;
            _bankingApiService = bankingApiService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("Pending")]
        public IActionResult Pending()
        {
            return View();
        }


        [HttpGet]
        [Route("CallBack/{depositWithdrawRequestId}")]
        public async Task<IActionResult> Index(int depositWithdrawRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found. Ensure the user is logged in.");
            }
            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(depositWithdrawRequestId, userId);
            if (depositWithdrawRequest == null)
            {
                return NotFound("Request not found or does not belong to the user.");
            }

            return View("Index", depositWithdrawRequest);
        }


        [HttpPost]
        [Route("Confirm/{depositWithdrawRequestId}")]
        public async Task<IActionResult> ConfirmRequest(int depositWithdrawRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found. Ensure the user is logged in.");
            }
            try
            {
                DepositWithdrawRequest? depositRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(depositWithdrawRequestId, userId);

                if (depositRequest == null)
                {
                    return NotFound("Request not found or does not belong to the user.");
                }
                string secretKey = _configuration["AppSettings:SecretKey"]!;
                string MerchantId = _configuration["AppSettings:MerchantId"]!;
                string hash = HashingHelper.GenerateSHA256Hash(depositRequest.Amount.ToString(), depositWithdrawRequestId.ToString(), secretKey, MerchantId);

                // Transaction been registered HERE
                await _bankingApiService.CallFinishDepositBankingApiAsync(depositRequest.Amount, depositRequest.UserId, depositWithdrawRequestId, hash);

                return View("Index", depositRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError("Database operation failed: {Message}", ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("CallBack/Withdraw/Confirm")]
        public async Task<IActionResult> ConfirmWithdrawRequest([FromBody] IncomingAdminWithdrawRequestDto req)
        {
            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAsync(req.RequestId);
            if (depositWithdrawRequest == null)
            {
                return NotFound("Request not found or does not belong to the user.");
            }

            var resp = await _transactionRepo.CreateWithdrawTransactionAsync(depositWithdrawRequest.UserId, req.Status, depositWithdrawRequest.Amount, req.RequestId);

            if(resp.Item2 != "OK")
            {
                bool fromAdmin = false;
                // Status Always Rejected passed to repo
                await _depWithRepo.UpdateWithdrawStatusAsync(req.RequestId, depositWithdrawRequest.Amount, fromAdmin);
            }

            return Ok(new { message = resp.Item2, requestId = depositWithdrawRequest.Id });
        }
    }
}
