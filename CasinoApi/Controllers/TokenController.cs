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
            string resp = await _tokenRepo.GeneratePrivateTokenAsync(req.UserId, req.PublicToken);
            return Ok(new {PrivateToken = resp, StatusCode = resp =="ERROR" ? 500 : 200 });
        }

        [HttpPost("Retrieve")]
        public async Task<IActionResult> RetrievePrivateToken(string publicToken)
        {
            string privateToken = await _tokenRepo.RetrievePrivateToken(publicToken);
            if(privateToken == null || privateToken == "ERROR")
            {
                return StatusCode(404, new {Message = "PrivateToken Not Found"});
            }
            return Ok(new {PrivateToken =  privateToken, Message = "Private token retrieved succeddfully!!!"});
        }

    }


}
