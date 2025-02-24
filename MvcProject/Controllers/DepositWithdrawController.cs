﻿using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MvcProject.Helpers;
using MvcProject.Models;
using MvcProject.Repositories;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    [Authorize]
    public class DepositWithdrawController : Controller
    {
        private readonly IDepositWithdrawRepository _depWithRepo;
        private readonly BankingApiService _bankingApiService;
        private readonly ILogger<DepositWithdrawController> _logger;

        public DepositWithdrawController(IDepositWithdrawRepository depWithRepo, BankingApiService bankingApiService, ILogger<DepositWithdrawController> logger)
        {
            _depWithRepo = depWithRepo;
            _bankingApiService = bankingApiService;
            _logger = logger;
        }

        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDeposit(decimal amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not logged in.");
            }
            try
            {
                // EVERYTHING HAPPENS IN SERVICE
                var bankingApiResponse = await _bankingApiService.CallDepositBankingApiAsync(amount, userId);
                if(bankingApiResponse.Status == "Rejected")
                {
                    return Ok(new { message = "Hash incorrect!! Someone is Between" });
                }
                return Ok(new { redirectUrl = bankingApiResponse.RedirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error on deposit: {Message}", ex.Message);
                return StatusCode(500, "error during req");
            }
        }


        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitWithdraw(decimal amount)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User is not logged in." });
            }

            try
            {
                var depositWithdrawResp = await _depWithRepo.AddWithdrawRequestAsync(userId, amount);
                if (depositWithdrawResp.Item1 == 0)
                {
                    return Ok(new { Message = depositWithdrawResp.Item2 });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during withdrawal: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred during the withdrawal request, if error persists, contact to Support" });
            }
        }
    }
}
