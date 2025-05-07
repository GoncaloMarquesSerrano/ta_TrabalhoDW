using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class UsersMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Developer_DeveloperFk",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_User_UserId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_User_UserId",
                table: "Review");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Developer",
                table: "Developer");

            migrationBuilder.RenameTable(
                name: "Developer",
                newName: "MyUser");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MyUser",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "MyUser",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "MyUser",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MyUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MyUser",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "MyUser",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "MyUser",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "MyUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MyUser",
                table: "MyUser",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_MyUser_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_MyUser_UserId",
                table: "Purchase",
                column: "UserId",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_MyUser_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "MyUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_MyUser_DeveloperFk",
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

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "MyUser");

            migrationBuilder.RenameTable(
                name: "MyUser",
                newName: "Developer");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Developer",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Developer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Developer",
                table: "Developer",
                column: "id");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Developer_DeveloperFk",
                table: "Game",
                column: "DeveloperFk",
                principalTable: "Developer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_User_UserId",
                table: "Purchase",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_User_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
