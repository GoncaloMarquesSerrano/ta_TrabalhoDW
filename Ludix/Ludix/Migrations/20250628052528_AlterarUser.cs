using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class AlterarUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game");

            migrationBuilder.AlterColumn<string>(
                name: "Cover",
                table: "Game",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

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

            migrationBuilder.AlterColumn<string>(
                name: "Cover",
                table: "Game",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
