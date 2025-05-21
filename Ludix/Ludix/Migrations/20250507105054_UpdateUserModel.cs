using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_MyUser_DeveloperFk",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "MyUser");

            migrationBuilder.CreateTable(
                name: "Developer",
                columns: table => new
                {
                    DeveloperId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AspNetUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AspNetUserUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developer", x => x.DeveloperId);
                    table.ForeignKey(
                        name: "FK_Developer_MyUser_AspNetUserUserId",
                        column: x => x.AspNetUserUserId,
                        principalTable: "MyUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Developer_AspNetUserUserId",
                table: "Developer",
                column: "AspNetUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Developer_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "Developer",
                principalColumn: "DeveloperId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developer_DeveloperFk",
                table: "Game");

            migrationBuilder.DropTable(
                name: "Developer");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MyUser",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "MyUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_MyUser_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
