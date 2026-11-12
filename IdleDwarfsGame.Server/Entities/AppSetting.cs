// Служебные значения сервера «ключ → строка», не относящиеся к игрокам.
// Например, SquadPowerBuild — сборка, при которой в последний раз пересчитывали мощь всех отрядов (см. Program.cs)
public class AppSetting
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
