using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("heroes")]
public class HeroesController(
    IHeroesRepository heroesRepository,
    IExpeditionRepository expeditionRepository,
    IEntityFactory entityFactory) : ControllerBase
{
    // Инвентарь залогиненного игрока: все его герои, у тех что в отряде заполнен PlaceInSquad
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<List<HeroInfo>>> GetMyHeroes()
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        var heroes = await heroesRepository.GetByPlayerAsync(playerId);

        // Аккаунты, созданные до появления стартового отряда, получают его при первом заходе в инвентарь
        if (heroes.Count == 0)
        {
            heroes = StarterSquad.Create(playerId);
            foreach (var hero in heroes)
                await heroesRepository.CreateAsync(hero);
        }

        return Ok(await ToInfoAsync(playerId, heroes));
    }

    [Authorize]
    [HttpPut("me/squad")]
    public async Task<ActionResult<List<HeroInfo>>> SaveMySquad([FromBody] SaveSquadRequest request)
    {
        if (CurrentPlayerId() is not { } playerId)
            return Unauthorized();

        try
        {
            var heroes = await heroesRepository.SaveSquadAsync(playerId, request.Slots);
            return Ok(await ToInfoAsync(playerId, heroes));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

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
        hero.Level = Math.Clamp(hero.Level, HeroLevels.Min, HeroLevels.Max);
        hero.Element = HeroLevels.NormalizeElement(hero.Level, hero.Element);
        await heroesRepository.CreateAsync(hero);
        return Ok(hero);
    }

    [HttpPost("{heroId:guid}/squad")]
    public async Task<IActionResult> AssignToSquad(Guid heroId, [FromQuery] int? place)
    {
        await heroesRepository.AssignToSquadAsync(heroId, place);
        return Ok();
    }

    private async Task<List<HeroInfo>> ToInfoAsync(Guid playerId, IEnumerable<Hero> heroes)
    {
        var busy = await expeditionRepository.GetBusyHeroIdsAsync(playerId);
        return heroes
            .Select(hero => HeroInfo.From(hero, CombatPower.OfHero(entityFactory, hero), busy.Contains(hero.Id)))
            .ToList();
    }

    private Guid? CurrentPlayerId() =>
        Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId) ? playerId : null;
}
