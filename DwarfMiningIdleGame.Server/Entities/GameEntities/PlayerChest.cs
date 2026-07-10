namespace DwarfMiningIdleGame.Server.Entities.GameEntities
{
    public class PlayerChest
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid ChestDropTableId { get; set; }
        public bool IsOpened { get; set; }

        public Player Player { get; set; }
        public ChestDropTable ChestDropTable { get; set; }
    }
}