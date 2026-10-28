// Бой PvP: нападавший против сохранённого отряда защитника (защитник в бою не участвует).
// Победы за сутки (Day — 00:00 UTC) убирают защитника из подбора нападавшего до конца суток:
// уникальный индекс (AttackerUserId, DefenderUserId, Day) по победам не даст победить его дважды
public class PvpBattle
{
    public Guid Id { get; set; }
    public Guid AttackerUserId { get; set; }
    public Guid DefenderUserId { get; set; }
    public DateTime FoughtAt { get; set; }
    public DateTime Day { get; set; }
    public bool AttackerWon { get; set; }

    // Мощь отрядов на момент боя — по ним считались очки
    public int AttackerPower { get; set; }
    public int DefenderPower { get; set; }

    // Изменение очков нападавшего во всех сезонах (поражение — отрицательное)
    public int Points { get; set; }
}

// Очки игрока в одном сезоне. Ключ (UserId, Type, Start): новый сезон — новая строка с нуля.
// Очки не опускаются ниже нуля
public class PvpSeasonScore
{
    public Guid UserId { get; set; }
    public PvpSeasonType Type { get; set; }
    public DateTime Start { get; set; }

    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    // При равных очках выше тот, кто набрал их раньше
    public DateTime UpdatedAt { get; set; }
}
