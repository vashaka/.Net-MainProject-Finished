using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITransactionRepository _transactionRepo;

        public HomeController(ILogger<HomeController> logger, ITransactionRepository transactionRepo)
        {
            _logger = logger;
            _transactionRepo = transactionRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Transactions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || userId == string.Empty)
            {
                return Unauthorized("User is not logged in.");
            }
            IEnumerable<TransactionDto> myTransactions = await _transactionRepo.GetAllMyTransactions(userId);
            return View(myTransactions);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
