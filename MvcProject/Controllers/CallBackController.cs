using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    //[Authorize]
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
            Console.WriteLine("userID: " + userId);
            Console.WriteLine("reqId: " + depositWithdrawRequestId);
            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(depositWithdrawRequestId, userId);
            Console.WriteLine(depositWithdrawRequest);
            if (depositWithdrawRequest == null)
            {
                return NotFound("Request not found or does not belong to the user.");
            }

            return View("Index", depositWithdrawRequest);
        }

// CALLING FROM BANKINGAPI
        [HttpPost]
        [Route("CallBack/Deposit/Confirm")]
        public async Task<IActionResult> ConfirmRequest([FromBody] IncomigRequesst req)
        {
            string userId = req.userid;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID not found. Ensure the user is logged in..");
                return Unauthorized("User ID not found. Ensure the user is logged in.");
            }

            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(req.reqid, userId);
            if (depositWithdrawRequest == null)
            {
                Console.WriteLine("Request not found or does not belong to the user.");
                return NotFound("Request not found or does not belong to the user.");
            }
            Console.WriteLine("Status from CallBack");
            Console.WriteLine(req.status);
            await _transactionRepo.CreateDepositTransactionAsync(userId, req.status, depositWithdrawRequest.Amount, req.reqid);


            return Ok(new { message = "Request confirmed successfully", requestId = depositWithdrawRequest.Id });
        }

        [HttpPost]
        [Route("CallBack/Withdraw/Confirm")]
        public async Task<IActionResult> ConfirmWithdrawRequest([FromBody] IncomigRequesst req)
        {
            string userId = req.userid;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID not found. Ensure the user is logged in..");
                return Unauthorized("User ID not found. Ensure the user is logged in.");
            }

            DepositWithdrawRequest? depositWithdrawRequest = await _depWithRepo.GetRequestByIdAndUserIdAsync(req.reqid, userId);
            if (depositWithdrawRequest == null)
            {
                Console.WriteLine("Request not found or does not belong to the user.");
                return NotFound("Request not found or does not belong to the user.");
            }
            string resp = await _transactionRepo.CreateWithdrawTransactionAsync(userId, req.status, depositWithdrawRequest.Amount, req.reqid);
            if(resp == "ERROR")
            {
                await _depWithRepo.UpdateDepositWithdrawStatusAsync(req.reqid, "Rejected");
            }

            return Ok(new { message = "Request confirmed successfully", requestId = depositWithdrawRequest.Id });
        }



        public class IncomigRequesst
        {
            public int reqid {  get; set; }
            public string userid { get; set; }
            public string status { get; set; }
        }



    }
}
