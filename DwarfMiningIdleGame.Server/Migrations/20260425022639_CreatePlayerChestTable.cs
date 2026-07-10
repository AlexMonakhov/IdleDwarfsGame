using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DwarfMiningIdleGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatePlayerChestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration is intentionally empty - PlayerChests table is already created
            // in migration 20260416194440_AddDropEntry.cs
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is intentionally empty
        }
    }
}
