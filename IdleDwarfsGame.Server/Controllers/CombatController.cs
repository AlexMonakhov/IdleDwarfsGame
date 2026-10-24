using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("combat")]
public class CombatController(CombatService combatService) : ControllerBase
{
    // Пока нет авторизации — по умолчанию бьётся демо-игрок из сидов
    [HttpPost("start")]
    public async Task<IActionResult> StartCombat([FromQuery] Guid? playerId, [FromQuery] int floor = 2)
    {
        try
        {
            var result = await combatService.StartTowerFight(playerId ?? HeroSeedData.DemoPlayerId, Math.Max(1, floor));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("boss")]
    public async Task<IActionResult> StartBossCombat([FromQuery] Guid? playerId, [FromQuery] int difficulty = 1)
    {
        try
        {
            var result = await combatService.StartBossFight(playerId ?? HeroSeedData.DemoPlayerId, Math.Max(1, difficulty));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
