using CasinoApi.Models;
using CasinoApi.Repos;
using Microsoft.AspNetCore.Mvc;

namespace CasinoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ActionsController : ControllerBase
    {
        private readonly IActionsRepository _actionsRepo;
        public ActionsController(IActionsRepository actionsRepo) 
        {
            _actionsRepo = actionsRepo;
        }

        [HttpPost("Bet")]
        public async Task<IActionResult> Bet(BetRequest betReq)
        {
            try
            {
                if (betReq == null || string.IsNullOrWhiteSpace(betReq.Token) || betReq.Amount <= 0)
                {
                    return StatusCode(411, new { Message = "Invalid Parameters", StatusCode = 411 });
                }

                var resp = await _actionsRepo.MakeBetAsync(betReq.Token, betReq.Amount, betReq.TransactionId, betReq.GameId, betReq.RoundId);

                if (resp.Item1 == 0)
                {
                    return StatusCode(resp.Item2, new { Message = resp.Item3, StatusCode = resp.Item2 });
                }

                return Ok(new
                {
                    Message = resp.Item3,
                    StatusCode = resp.Item2,
                    Data = new
                    {
                        TransactionsId = betReq.TransactionId,
                        UpdatedBalance = resp.Item1,
                    }

                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }


        [HttpPost("Win")]
        public async Task<IActionResult> Win(WinRequest winReq)
        {
            try
            {
                if (winReq == null || string.IsNullOrWhiteSpace(winReq.Token) || winReq.Amount <= 0)
                {
                    return StatusCode(411, new { Message = "Invalid Parameters", StatusCode = 411 });
                }

                var resp = await _actionsRepo.WinAsync(winReq.Token, winReq.Amount, winReq.TransactionId, winReq.GameId, winReq.RoundId);

                if (resp.Item1 == 0)
                {
                    return StatusCode(resp.Item2, new { Message = resp.Item3, StatusCode = resp.Item2 });
                }

                return Ok(new
                {
                    Message = resp.Item3,
                    StatusCode = resp.Item2,
                    Data = new
                    {
                        TransactionsId = winReq.TransactionId,
                        UpdatedBalance = resp.Item1,
                    }

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        [HttpPost("CancelBet")]
        public async Task<IActionResult> CancelBet(CancelBetRequest cancelBetReq)
        {
            try
            {
                if (cancelBetReq == null || string.IsNullOrWhiteSpace(cancelBetReq.Token))
                {
                    return StatusCode(411, new { Message = "Invalid Parameters", StatusCode = 411 });
                }

                var resp = await _actionsRepo.CancelBetAsync(cancelBetReq.Token, cancelBetReq.TransactionId,cancelBetReq.BetTransactionId, cancelBetReq.Amount, cancelBetReq.GameId, cancelBetReq.RoundId);

                if (resp.Item1 == 0)
                {
                    return StatusCode(resp.Item2, new { Message = resp.Item3, StatusCode = resp.Item2 });
                }

                return Ok(new
                {
                    Message = resp.Item3,
                    StatusCode = resp.Item2,
                    Data = new
                    {
                        TransactionsId = cancelBetReq.TransactionId,
                        UpdatedBalance = resp.Item1,
                    }

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost("ChangeWin")]
        public async Task<IActionResult> ChangeWin(ChangeWinRequest changeWinReq)
        {
            try
            {
                if (changeWinReq == null || string.IsNullOrWhiteSpace(changeWinReq.Token))
                {
                    return StatusCode(411, new { Message = "Invalid Parameters", StatusCode = 411 });
                }

                var resp = await _actionsRepo.ChangeWinAsync(changeWinReq.Token, changeWinReq.TransactionId, changeWinReq.PreviousTransactionId, changeWinReq.Amount, changeWinReq.PreviousAmount, changeWinReq.GameId, changeWinReq.RoundId);

                if (resp.Item1 == 0)
                {
                    return StatusCode(resp.Item2, new { Message = resp.Item3, StatusCode = resp.Item2 });
                }

                return Ok(new
                {
                    Message = resp.Item3,
                    StatusCode = resp.Item2,
                    Data = new
                    {
                        TransactionsId = changeWinReq.TransactionId,
                        UpdatedBalance = resp.Item1,
                    }

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost("GetBalance")]
        public async Task<IActionResult> GetBalance(GetBalanceRequest GetBalanceReq)
        {
            try
            {
                if (GetBalanceReq == null || string.IsNullOrWhiteSpace(GetBalanceReq.Token))
                {
                    return StatusCode(411, new { Message = "Invalid Parameters", StatusCode = 411 });
                }

                var resp = await _actionsRepo.GetBalanceAsync(GetBalanceReq.Token);

                if (resp.Item1 == 0)
                {
                    return StatusCode(resp.Item2, new { Message = resp.Item3, StatusCode = resp.Item2 });
                }

                return Ok(new
                {
                    Message = resp.Item3,
                    StatusCode = resp.Item2,
                    Data = new 
                    { 
                        CurrentBalance = resp.Item1,
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost("GetPlayerInfo")]
        public async Task<IActionResult> GetPlayerInfo(string privateToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(privateToken))
                {
                    return StatusCode(411, new { Message = "Invalid Parameters", StatusCode = 411 });
                }

                var resp = await _actionsRepo.GetUserInfoAsync(privateToken);

                if (resp.Item1 == null)
                {
                    return StatusCode(resp.Item2, new { Message = resp.Item3, StatusCode = resp.Item2 });
                }

                return Ok(new
                {
                    Message = resp.Item3,
                    StatusCode = resp.Item2,
                    Data = new
                    {
                        UserId = resp.Item1?.Id ?? "",
                        UserName = resp.Item1?.UserName ?? "",
                        Email = resp.Item1?.Email ?? "",
                        CurrentBalance = resp.Item1?.CurrentBalance ?? 0,
                        Currency = resp.Item1?.Currency == 1 ? "GEL" :
                                   resp.Item1?.Currency == 2 ? "USD" :
                                   resp.Item1?.Currency == 3 ? "EUR" : "Unknown"
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }
    }
}
