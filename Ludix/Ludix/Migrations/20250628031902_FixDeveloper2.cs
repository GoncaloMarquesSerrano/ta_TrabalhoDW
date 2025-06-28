using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class FixDeveloper2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "Developers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "Developers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
