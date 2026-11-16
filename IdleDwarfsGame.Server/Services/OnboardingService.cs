using Microsoft.EntityFrameworkCore;

// Состояние стартового обучения и его выдачи (правила и суммы — Onboarding). Выдача «один раз» отмечается ключом
// Onboarding.Grants.* в PlayerTutorial: повторный запрос, перезагрузка и пропуск ничего не выдадут второй раз
public class OnboardingService(
    GameDbContext db,
    ITutorialRepository tutorials,
    SquadPowerService squadPower,
    Random random,
    TimeProvider time)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<bool> IsActiveAsync(Guid playerId) =>
        await tutorials.GetOneAsync(playerId, Onboarding.Key) is { Status: null };

    public async Task<OnboardingState> GetStateAsync(Guid playerId)
    {
        var row = await tutorials.GetOneAsync(playerId, Onboarding.Key);
        return new OnboardingState { Active = row is { Status: null }, Step = row?.Step ?? 0 };
    }

    // Выдача во время обучения: true — обучение идёт и этой выдачи ещё не было (отмечена сейчас)
    public async Task<bool> TryGrantAsync(Guid playerId, string grant) =>
        await IsActiveAsync(playerId) && await tutorials.TryMarkDoneAsync(playerId, grant, Now);

    // Отметить выдачу, идёт обучение или нет (награда уже начатой обучающей экспедиции): true — отмечена сейчас
    public Task<bool> TryMarkGrantAsync(Guid playerId, string grant) => tutorials.TryMarkDoneAsync(playerId, grant, Now);

    // Выдача уже была (или занята сейчас): не зависит от того, идёт ли обучение
    public async Task<bool> WasGrantedAsync(Guid playerId, string grant) =>
        await tutorials.GetOneAsync(playerId, grant) is { DoneAt: not null };

    public async Task SetStepAsync(Guid playerId, int step)
    {
        if (!await IsActiveAsync(playerId))
            throw new TutorialException("Onboarding is not active");
        await tutorials.SetStepAsync(playerId, Onboarding.Key, Math.Max(0, step), Now);
    }

    public async Task CompleteAsync(Guid playerId)
    {
        if (!await IsActiveAsync(playerId))
            return;
        foreach (var activity in Onboarding.CoveredActivities)
            await tutorials.SetStatusAsync(playerId, activity, TutorialStatus.Completed, Now);
        await tutorials.SetStatusAsync(playerId, Onboarding.Key, TutorialStatus.Completed, Now);
    }

    // Дворфы стартового сундука — один раз на аккаунт (пропуск мог уже выдать их напрямую). Отряда ещё нет —
    // пятеро встают в него; есть (старый аккаунт) — все в запас
    public async Task<List<Hero>> StarterChestHeroesAsync(Guid playerId)
    {
        if (!await tutorials.TryMarkDoneAsync(playerId, Onboarding.Grants.Heroes, Now))
            return [];
        var heroes = Onboarding.StarterHeroes(playerId, random);
        if (await db.Heroes.AnyAsync(h => h.PlayerId == playerId && h.PlaceInSquad != null))
            heroes.ForEach(h => h.PlaceInSquad = null);
        return heroes;
    }

    public Task RefreshSquadPowerAsync(Guid playerId) => squadPower.RefreshAsync(playerId);

    // Пропуск: всё, что обучение выдало бы по дороге, — сразу. Сундук не нужен: дворфы приходят напрямую
    public async Task SkipAsync(Guid playerId)
    {
        if (!await IsActiveAsync(playerId))
            return;

        var now = Now;
        long gold = 0, experience = 0;

        var heroes = await StarterChestHeroesAsync(playerId);
        if (heroes.Count > 0)
        {
            db.Heroes.AddRange(heroes);
            await db.SaveChangesAsync();
            await squadPower.RefreshAsync(playerId);
        }

        if (await tutorials.TryMarkDoneAsync(playerId, Onboarding.Grants.StageReward, now))
        {
            gold += Onboarding.StageGold;
            experience += Onboarding.StageExperience;
        }

        if (await tutorials.TryMarkDoneAsync(playerId, Onboarding.Grants.Expedition, now))
        {
            gold += Onboarding.ExpeditionGold;
            experience += Onboarding.ExpeditionExperience;
        }

        if (await tutorials.TryMarkDoneAsync(playerId, Onboarding.Grants.Mine, now))
        {
            await GrantMineAsync(playerId, now);
            gold += Onboarding.MineGold;
        }

        if (gold > 0 || experience > 0)
        {
            await db.Players.Where(p => p.Id == playerId).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Gold, p => p.Gold + gold)
                .SetProperty(p => p.Experience, p => p.Experience + experience));
        }

        // Обучающий бой засчитан: 1-й уровень пройден (его награду пропуск уже выдал — строки награды нет)
        if (!await db.PlayerCampaignProgress.AnyAsync(p => p.PlayerId == playerId && p.StageId == TutorialBattle.StageId))
        {
            db.PlayerCampaignProgress.Add(new PlayerCampaignProgress { PlayerId = playerId, StageId = TutorialBattle.StageId, ClearedAt = now });
            await db.SaveChangesAsync();
        }

        // Пропустил обучение — знакомства с этими зданиями не нужны, и подсказка при входе туда не ведёт
        foreach (var activity in Onboarding.CoveredActivities)
        {
            await tutorials.SetStatusAsync(playerId, activity, TutorialStatus.Skipped, now);
            await tutorials.MarkDoneAsync(playerId, activity, now);
        }

        await tutorials.SetStatusAsync(playerId, Onboarding.Key, TutorialStatus.Skipped, now);
    }

    // Шахта, которую обучение построило бы: золотая, 2-го уровня, с дворфом из запаса
    private async Task GrantMineAsync(Guid playerId, DateTime now)
    {
        var mines = await db.Mines.Include(m => m.Dwarves).Where(m => m.PlayerId == playerId).ToListAsync();
        var mine = mines.FirstOrDefault(m => m.Type == Onboarding.MineKind);
        if (mine == null)
        {
            var taken = mines.Select(m => m.Plot).ToHashSet();
            int plot = Enumerable.Range(1, MiningService.PlotCount).FirstOrDefault(p => !taken.Contains(p));
            if (plot == 0)
                return;
            mine = new Mine { Id = Guid.NewGuid(), PlayerId = playerId, Plot = plot, Type = Onboarding.MineKind, UpdatedAt = now };
            db.Mines.Add(mine);
        }

        mine.Level = Math.Max(mine.Level, MiningService.DwarfUnlockLevel);
        if (mine.Dwarves.Count == 0 && await FreeDwarfAsync(playerId) is { } heroId)
        {
            // Добытое до прихода дворфа — по старому составу (без дворфа — ничего)
            mine.Stored = MiningService.StoredAt(mine, now);
            mine.UpdatedAt = now;
            mine.Dwarves.Add(new MineDwarf { MineId = mine.Id, HeroId = heroId, Slot = 1 });
        }
        mine.Version++;
        await db.SaveChangesAsync();
    }

    // Дворф не из отряда, не в экспедиции и не в шахте
    private Task<Guid?> FreeDwarfAsync(Guid playerId) =>
        db.Heroes
            .Where(h => h.PlayerId == playerId && h.PlaceInSquad == null
                && !db.ExpeditionHeroes.Any(e => e.HeroId == h.Id)
                && !db.MineDwarves.Any(d => d.HeroId == h.Id))
            .OrderBy(h => h.Level)
            .Select(h => (Guid?)h.Id)
            .FirstOrDefaultAsync();
}

public class OnboardingState
{
    public bool Active { get; set; }
    public int Step { get; set; }
}
