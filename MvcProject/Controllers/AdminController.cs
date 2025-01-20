﻿using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Repositories;
using System.Text;
using System.Linq;
using MvcProject.Helpers;


namespace MvcProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly IDepositWithdrawRepository _depositWithdrawRepo;
        private readonly BankingApiService _bankingApiService;

        public AdminController(IAdminRepository adminRepo, IDepositWithdrawRepository depositWithdrawRepo, BankingApiService bankingApiService)
        {
            _adminRepo = adminRepo;
            _depositWithdrawRepo = depositWithdrawRepo;
            _bankingApiService = bankingApiService;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<DepositWithdrawRequestDto> getAllRequests = await _adminRepo.GetAllDepositWithdrawRequestsAsync();
            IEnumerable<DepositWithdrawRequestDto> sorted = getAllRequests.Where(r => r.TransactionType == "Withdraw").OrderByDescending(r => r.CreatedAt).ToList();

            return View(sorted);
        }

        [HttpPost]
        public async Task<IActionResult> AdminApproveReject(int id, decimal amount, string status, string transactionType, string userId)
        {
            if (status == "Rejected")
            {
                bool fromAdmin = true;
                // Status Always rejected passed to Repo
                await _depositWithdrawRepo.UpdateWithdrawStatusAsync(id, amount, fromAdmin);
                return Ok();
            }
            const string secretKey = "vashaka_secret_keyy";
            string hash = HashingHelper.GenerateSHA256Hash(amount.ToString(), id.ToString(),secretKey);


            await _bankingApiService.CallAdminBankingApi(id, amount, status, transactionType, hash);
            return Ok();
        }
    }
}
