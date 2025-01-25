using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models;
using MvcProject.Repositories;
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
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        //public class GenerateTokenRequest
        //{
        //    public string Guid { get; set; }
        //}

        //[HttpPost("Generate")]
        //public StatusResponse GeneratePrivateToken([FromBody] GenerateTokenRequest req)
        //{
        //    Console.WriteLine(req.Guid + " HElooooo");
        //    StatusResponse response = new()
        //    {
        //        PrivateToken = Guid.NewGuid().ToString(),
        //        StatusCode = 0
        //    };
        //    return response;
        //}

        //public class StatusResponse
        //{
        //    public string PrivateToken { get; set; }
        //    public int StatusCode { get; set; }
        //}

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

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterTransactions(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || userId == string.Empty)
            {
                return Unauthorized("User is not logged in.");
            }
            var filteredTransactions = await _transactionRepo.GetAllMyTransactions(userId);

            if (startDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.CreatedAt <= endDate.Value);
            }

            var result = filteredTransactions.Select(t => new
            {
                t.Id,
                t.TransactionType,
                t.Amount,
                t.Currency,
                t.Status,
                CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            return Json(result);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
