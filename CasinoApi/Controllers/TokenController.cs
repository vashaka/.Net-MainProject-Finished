using Azure.Core;
using CasinoApi.Models;
using CasinoApi.Repos;
using Microsoft.AspNetCore.Mvc;

namespace CasinoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenRepository _tokenRepo;
        public TokenController(ITokenRepository tokenRepo)
        {
            _tokenRepo = tokenRepo;
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> GeneratePrivateToken([FromBody] GenerateTokenRequest req)
        {
            var resp = await _tokenRepo.GeneratePrivateTokenAsync(req.UserId, req.PublicToken);
            return Ok(new {ReturnMessage = resp.Item1, StatusCode = resp.Item2 });
        }

        [HttpPost("RetrieveAndActivatePrivateToken")]
        public async Task<IActionResult> ActivatePrivateToken(string publicToken)
        {
            var resp = await _tokenRepo.ActivatePrivateTokenAsync(publicToken);
            
            if(resp.Item1 == null || resp.Item3 != 200)
            {
                return StatusCode(resp.Item3, new {Message = resp.Item2});
            }
            return Ok(new {PrivateToken =  resp.Item1, Message = resp.Item2});
        }

    }


}
