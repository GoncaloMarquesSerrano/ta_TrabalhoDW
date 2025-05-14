using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class DeveloperMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Developer_MyUser_AspNetUserUserId",
                table: "Developer");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developer_DeveloperFk",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_MyUser_UserId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_MyUser_UserId",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MyUser",
                table: "MyUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Developer",
                table: "Developer");

            migrationBuilder.DropIndex(
                name: "IX_Developer_AspNetUserUserId",
                table: "Developer");

            migrationBuilder.DropColumn(
                name: "DeveloperId",
                table: "Developer");

            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "Developer");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Developer");

            migrationBuilder.RenameTable(
                name: "MyUser",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Developer",
                newName: "Developers");

            migrationBuilder.RenameColumn(
                name: "AspNetUserUserId",
                table: "Developers",
                newName: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Developers",
                table: "Developers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Developers_Users_UserId",
                table: "Developers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "Developers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Users_UserId",
                table: "Purchase",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Users_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Developers_Users_UserId",
                table: "Developers");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developers_DeveloperFk",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Users_UserId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Users_UserId",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Developers",
                table: "Developers");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "MyUser");

            migrationBuilder.RenameTable(
                name: "Developers",
                newName: "Developer");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Developer",
                newName: "AspNetUserUserId");

            migrationBuilder.AddColumn<int>(
                name: "DeveloperId",
                table: "Developer",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "Developer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Developer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MyUser",
                table: "MyUser",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Developer",
                table: "Developer",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_Developer_AspNetUserUserId",
                table: "Developer",
                column: "AspNetUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Developer_MyUser_AspNetUserUserId",
                table: "Developer",
                column: "AspNetUserUserId",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Developer_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "Developer",
                principalColumn: "DeveloperId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_MyUser_UserId",
                table: "Purchase",
                column: "UserId",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_MyUser_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
