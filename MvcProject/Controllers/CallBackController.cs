using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [Authorize]
    public class CallBackController : Controller
    {
        private readonly IDepositWithdrawRepository _depWithRepo;
        private readonly ITransactionRepository _transactionRepo;
        public CallBackController(IDepositWithdrawRepository depWithRepo, ITransactionRepository transactionRepo)
        {
            _depWithRepo = depWithRepo;
            _transactionRepo = transactionRepo;
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
            if (depositWithdrawRequest == null || depositWithdrawRequest.Status != "Pending")
            {
                return NotFound("Request not found or does not belong to the user.");
            }

            return View("Index", depositWithdrawRequest);
        }

        [HttpPost]
        [Route("CallBack/Confirm")]
        public async Task<IActionResult> ConfirmRequest([FromForm] int DepositWithdrawRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found. Ensure the user is logged in.");
            }

            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(DepositWithdrawRequestId, userId);
            if (depositWithdrawRequest == null)
            {
                return NotFound("Request not found or does not belong to the user.");
            }

            depositWithdrawRequest.Status = "Success";
            await _transactionRepo.CreateTransactionAsync(userId, depositWithdrawRequest.Status, depositWithdrawRequest.Amount, DepositWithdrawRequestId);


            return Ok(new { message = "Request confirmed successfully", requestId = depositWithdrawRequest.Id });
        }


    }
}
