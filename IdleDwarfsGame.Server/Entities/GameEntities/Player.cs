public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rating { get; set; }

    // Общие на аккаунт ресурсы
    public long Gold { get; set; }
    public long Experience { get; set; }

    // Мощь отряда защиты — героев с местом в отряде, против них дерутся в PvP (CombatPower.OfSquad).
    // Хранится, чтобы подбор соперников не собирал отряды всех игроков на каждый запрос.
    // Пересчитывает SquadPowerService.RefreshAsync после всего, что меняет героев отряда; на старте сервера — у всех
    public int SquadPower { get; set; }
}
