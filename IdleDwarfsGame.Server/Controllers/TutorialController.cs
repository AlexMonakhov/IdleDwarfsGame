using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Прогресс обучения: статусы сценариев и попробованные активности (TutorialService)
[Authorize]
[ApiController]
[Route("tutorial")]
public class TutorialController(TutorialService tutorialService, OnboardingService onboardingService) : ControllerBase
{
    // Стартовое обучение: шаг, на котором игрок остановился (продолжение после перезагрузки)
    [HttpPost("onboarding/step/{step:int}")]
    public Task<ActionResult<TutorialProgress>> OnboardingStep(int step) =>
        Run(async playerId =>
        {
            await onboardingService.SetStepAsync(playerId, step);
            return await tutorialService.GetAsync(playerId);
        });

    [HttpPost("onboarding/complete")]
    public Task<ActionResult<TutorialProgress>> CompleteOnboarding() =>
        Run(async playerId =>
        {
            await onboardingService.CompleteAsync(playerId);
            return await tutorialService.GetAsync(playerId);
        });

    // Пропуск выдаёт всё, что обучение выдало бы по дороге (OnboardingService.SkipAsync), и помечает знакомыми
    // все окна, которые прислал фронт (список активностей — src/tutorial/activities.ts): их знакомства не запускаются
    [HttpPost("onboarding/skip")]
    public Task<ActionResult<TutorialProgress>> SkipOnboarding([FromBody] SkipOnboardingRequest? request) =>
        Run(async playerId =>
        {
            if (request?.Activities is { Count: > 0 } activities)
                await tutorialService.SetStatusesAsync(playerId, activities, TutorialStatus.Skipped);
            await onboardingService.SkipAsync(playerId);
            return await tutorialService.GetAsync(playerId);
        });

    [HttpGet]
    public Task<ActionResult<TutorialProgress>> Get() =>
        Run(playerId => tutorialService.GetAsync(playerId));

    [HttpPost("{key}/complete")]
    public Task<ActionResult<TutorialProgress>> Complete(string key) =>
        Run(async playerId =>
        {
            await tutorialService.SetStatusAsync(playerId, key, TutorialStatus.Completed);
            return await tutorialService.GetAsync(playerId);
        });

    [HttpPost("{key}/skip")]
    public Task<ActionResult<TutorialProgress>> Skip(string key) =>
        Run(async playerId =>
        {
            await tutorialService.SetStatusAsync(playerId, key, TutorialStatus.Skipped);
            return await tutorialService.GetAsync(playerId);
        });

    // Кнопки «Сбросить обучение» / «Начать заново» в аккаунте
    [HttpPost("reset")]
    public Task<ActionResult<TutorialProgress>> Reset() =>
        Run(async playerId =>
        {
            await tutorialService.ResetAsync(playerId);
            return await tutorialService.GetAsync(playerId);
        });

    private async Task<ActionResult<T>> Run<T>(Func<Guid, Task<T>> action)
    {
        if (!Guid.TryParse(User.FindFirstValue(AuthClaims.PlayerId), out var playerId))
            return Unauthorized();

        try
        {
            return Ok(await action(playerId));
        }
        catch (TutorialException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class SkipOnboardingRequest
{
    public List<string> Activities { get; set; } = new();
}
