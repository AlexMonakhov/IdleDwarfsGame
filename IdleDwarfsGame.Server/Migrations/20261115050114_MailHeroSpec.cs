using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class MailHeroSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroSpec",
                table: "MailAttachments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroSpec",
                table: "MailAttachments");
        }
    }
}
