using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DwarfMiningIdleGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcesAndHeroField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaceInSquad",
                table: "Heroes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerResourses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourseId = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    QtyInStack = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerResourses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resourses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ResourseType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resourses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerResourses");

            migrationBuilder.DropTable(
                name: "Resourses");

            migrationBuilder.DropColumn(
                name: "PlaceInSquad",
                table: "Heroes");
        }
    }
}
