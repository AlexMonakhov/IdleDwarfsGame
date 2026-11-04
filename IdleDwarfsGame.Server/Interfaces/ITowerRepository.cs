public interface ITowerRepository
{
    // Последний пройденный этаж за неделю; 0 — на этой неделе ещё ничего не пройдено
    Task<int> GetFloorAsync(Guid playerId, DateTime week);

    // Лучший результат прошлых недель (до week); 0 — его нет
    Task<int> GetBestBeforeAsync(Guid playerId, DateTime week);

    // Засчитать этажи: прогресс недели меняется с fromFloor на toFloor вместе с наградой, одним сохранением.
    // false — прогресс уже не fromFloor (этаж только что засчитал параллельный бой), ничего не сохранено
    Task<bool> AdvanceAsync(Guid playerId, DateTime week, int fromFloor, int toFloor, PlayerReward reward, DateTime now);
}
