using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("combat")]
public class CombatController(CombatService combatService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> StartCombat()
    {
        var result = await combatService.StartTowerFight(new Guid(), 2);
        return Ok(result);
    }
}