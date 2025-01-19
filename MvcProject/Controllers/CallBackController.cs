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

        public CallBackController(IDepositWithdrawRepository depWithRepo, ITransactionRepository transactionRepo, BankingApiService bankingApiService, ILogger<CallBackController> logger)
        {
            _depWithRepo = depWithRepo;
            _transactionRepo = transactionRepo;
            _bankingApiService = bankingApiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Pending()
        {
            return View();
        }


        [HttpGet]
        [Route("CallBack/{hash}/{depositWithdrawRequestId}")]
        public async Task<IActionResult> Index(string hash,int depositWithdrawRequestId)
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

                const string secretKey = "vashaka_secret_keyy";
                string hash = HashingHelper.GenerateSHA256Hash(depositRequest.Amount.ToString(),userId,depositWithdrawRequestId.ToString(),secretKey);

                // Transaction it is been registered in bankingApiservice
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
            if (string.IsNullOrEmpty(req.UserId))
            {
                _logger.LogInformation("User ID not found. Ensure the user is logged in..");
                return Unauthorized("User ID not found. Ensure the user is logged in.");
            }

            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(req.RequestId, req.UserId);
            if (depositWithdrawRequest == null)
            {
                return NotFound("Request not found or does not belong to the user.");
            }

            string resp = await _transactionRepo.CreateWithdrawTransactionAsync(req.UserId, req.Status, depositWithdrawRequest.Amount, req.RequestId);

            if(resp == "ERROR")
            {
                bool fromAdmin = false;
                // Status Always Rejected passed to repo
                await _depWithRepo.UpdateWithdrawStatusAsync(req.RequestId, depositWithdrawRequest.Amount, fromAdmin);
            }

            return Ok(new { message = "Request confirmed successfully", requestId = depositWithdrawRequest.Id });
        }
    }
}
