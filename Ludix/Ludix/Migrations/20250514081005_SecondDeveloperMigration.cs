using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class SecondDeveloperMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeveloperRequestDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProposedWebsite",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequestedDeveloper",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Developers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Developers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Developers_ApprovedByUserId",
                table: "Developers",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Developers_Users_ApprovedByUserId",
                table: "Developers",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Developers_Users_ApprovedByUserId",
                table: "Developers");

            migrationBuilder.DropIndex(
                name: "IX_Developers_ApprovedByUserId",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "DeveloperRequestDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProposedWebsite",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequestedDeveloper",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Developers");
        }
    }
}
