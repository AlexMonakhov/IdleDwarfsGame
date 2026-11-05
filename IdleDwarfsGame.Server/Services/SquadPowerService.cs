// Сохранённая мощь отряда защиты (Player.SquadPower) и снимков рейтинговой арены (RankedSquad.Power).
// По ней подбираются соперники PvP: без неё пришлось бы на каждый запрос собирать отряды всех игроков.
//
// Пересчёт дешёвый (до 5 героев из БД и формула, без боёв), но делается только когда нужен:
// RefreshAsync — после смены состава отряда, RefreshIfInSquadAsync — после изменения героев (прокачка, слияние,
// способности, предметы): если ни один из них не стоял в отряде, мощь отряда не изменилась и запроса нет.
// Новое действие над героями — не забыть вызвать и его.
// Подстраховка: обзор PvP пересчитывает мощь своего игрока, а старт сервера — всех (правки баланса меняют формулу)
public class SquadPowerService(IPvpRepository pvp, IEntityFactory entityFactory)
{
    public int Of(IEnumerable<Hero> squad) => CombatPower.OfSquad(entityFactory, squad);

    // Пересчитать после изменения этих героев — только если хоть один из них стоял в отряде
    // (герои загружены до изменения: так ловится и уход из отряда при слиянии или растворении)
    public async Task RefreshIfInSquadAsync(Guid playerId, IEnumerable<Hero> changed)
    {
        if (changed.Any(h => h.PlaceInSquad != null))
            await RefreshAsync(playerId);
    }

    // Пересчитать и сохранить (запись — только если значение изменилось). Возвращает актуальную мощь
    public async Task<int> RefreshAsync(Guid playerId)
    {
        var squads = await pvp.GetDefenseSquadsAsync([playerId]);
        int power = Of(squads.GetValueOrDefault(playerId) ?? []);
        await pvp.SetSquadPowerAsync(playerId, power);
        return power;
    }

    // Все игроки и все снимки рейтинговой арены; пишутся только изменившиеся. Отряды грузятся пачками
    public async Task RefreshAllAsync(int batchSize = 500)
    {
        var stored = await pvp.GetSquadPowersAsync();
        foreach (var batch in stored.Keys.Chunk(batchSize))
        {
            var squads = await pvp.GetDefenseSquadsAsync(batch);
            foreach (var playerId in batch)
            {
                int power = Of(squads.GetValueOrDefault(playerId) ?? []);
                if (power != stored[playerId])
                    await pvp.SetSquadPowerAsync(playerId, power);
            }
        }

        foreach (var squad in await pvp.GetAllRankedSquadsAsync())
        {
            int power = Of(squad.Heroes());
            if (power != squad.Power)
                await pvp.SetRankedSquadPowerAsync(squad.UserId, power);
        }
    }
}
