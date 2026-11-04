using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("combat")]
public class CombatController(CombatService combatService, IEntityFactory entityFactory, TimeProvider time) : ControllerBase
{
    // Пробный бой на арене: этаж бесконечной башни текущей недели, без прогресса и наград.
    // Пока нет авторизации — по умолчанию бьётся демо-игрок из сидов
    [HttpPost("start")]
    public async Task<IActionResult> StartCombat([FromQuery] Guid? playerId, [FromQuery] int floor = 2)
    {
        try
        {
            var towerFloor = InfinityTower.Floor(InfinityTower.WeekOf(time.GetUtcNow().UtcDateTime), Math.Max(1, floor));
            var result = await combatService.StartTowerFight(playerId ?? HeroSeedData.DemoPlayerId, new TowerFloorScenario(entityFactory, towerFloor));
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
