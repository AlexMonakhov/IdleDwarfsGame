using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Админ-панель: поиск игрока, каталог того, что можно вложить в письмо, отправка письма с вложениями.
// TODO: закрыть правами доступа (роль админа) — пока открыто всем, даже без входа
[AllowAnonymous]
[ApiController]
[Route("admin")]
public class AdminController(MailService mailService) : ControllerBase
{
    [HttpGet("players")]
    public Task<List<AdminPlayerInfo>> FindPlayers([FromQuery] string query = "") =>
        mailService.FindPlayersAsync(query);

    [HttpGet("mail/catalog")]
    public AdminMailCatalog Catalog() => mailService.Catalog();

    [HttpPost("mail/hero-preview")]
    public ActionResult<AdminHeroPreview> HeroPreview(HeroPreviewRequest request)
    {
        try
        {
            return Ok(mailService.PreviewHero(request.Level, request.Element ?? string.Empty, request.Seed, request.HeroSpec));
        }
        catch (MailException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("players/{playerId:guid}/mail")]
    public Task<List<MailInfo>> PlayerMail(Guid playerId) => mailService.GetMailForAdminAsync(playerId);

    [HttpPost("mail")]
    public async Task<ActionResult<MailInfo>> Send(SendMailRequest request)
    {
        try
        {
            var attachments = (request.Attachments ?? []).Select(a => new MailAttachment
            {
                Kind = a.Kind,
                Key = a.Key ?? string.Empty,
                Level = a.Level,
                Rarity = a.Rarity,
                AbilityKind = a.AbilityKind,
                Amount = a.Amount,
                Seed = a.Seed ?? 0,
                HeroSpec = a.HeroSpec
            }).ToList();
            return Ok(await mailService.SendAsync(request.PlayerId, request.Sender, request.Title ?? string.Empty, request.Body ?? string.Empty, attachments));
        }
        catch (MailException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
