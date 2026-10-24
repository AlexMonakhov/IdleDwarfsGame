using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("heroes")]
public class HeroesController(IHeroesRepository heroesRepository) : ControllerBase
{
    [HttpGet("{playerId:guid}")]
    public async Task<IActionResult> GetHeroes(Guid playerId)
    {
        return Ok(await heroesRepository.GetByPlayerAsync(playerId));
    }

    [HttpGet("{playerId:guid}/squad")]
    public async Task<IActionResult> GetSquad(Guid playerId)
    {
        return Ok(await heroesRepository.GetSquadAsync(playerId));
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddHero([FromBody] Hero hero)
    {
        await heroesRepository.CreateAsync(hero);
        return Ok(hero);
    }

    [HttpPost("{heroId:guid}/squad")]
    public async Task<IActionResult> AssignToSquad(Guid heroId, [FromQuery] int? place)
    {
        await heroesRepository.AssignToSquadAsync(heroId, place);
        return Ok();
    }
}
