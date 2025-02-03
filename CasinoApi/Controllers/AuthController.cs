using Azure.Core;
using CasinoApi.Models;
using CasinoApi.Repos;
using Microsoft.AspNetCore.Mvc;

namespace CasinoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenRepository _tokenRepo;
        public AuthController(ITokenRepository tokenRepo)
        {
            _tokenRepo = tokenRepo;
        }

        [HttpPost("Generate")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GeneratePrivateToken([FromBody] GenerateTokenRequest req)
        {
            var resp = await _tokenRepo.GeneratePrivateTokenAsync(req.UserId, req.PublicToken);
            return Ok(new {ReturnMessage = resp.Item1, StatusCode = resp.Item2 });
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePrivateToken(string publicToken)
        {
            var resp = await _tokenRepo.ActivatePrivateTokenAsync(publicToken);
            
            if(resp.Item1 == null || resp.Item3 != 200)
            {
                return StatusCode(resp.Item3, new {
                    //Message = resp.Item2, 
                    StatusCode = resp.Item3});
            }
            return Ok(new { StatusCode = resp.Item3, Data = new { PrivateToken = resp.Item1 }});
        }
    }
}
