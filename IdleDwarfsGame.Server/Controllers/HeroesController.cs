using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("heroes")]
public class HeroesController : ControllerBase
{
    [HttpPost("add")]
    public async Task<IActionResult> AddHero(Guid playerId)
    {
        
        return Ok();
    }
}