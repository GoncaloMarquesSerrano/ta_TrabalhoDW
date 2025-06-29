using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludix.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_Game_GameId",
                table: "WishlistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_Users_UserId",
                table: "WishlistItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WishlistItems",
                table: "WishlistItems");

            migrationBuilder.RenameTable(
                name: "WishlistItems",
                newName: "Wishlist");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_UserId",
                table: "Wishlist",
                newName: "IX_Wishlist_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_GameId",
                table: "Wishlist",
                newName: "IX_Wishlist_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wishlist",
                table: "Wishlist",
                column: "WishlistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlist_Game_GameId",
                table: "Wishlist",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlist_Users_UserId",
                table: "Wishlist",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wishlist_Game_GameId",
                table: "Wishlist");

            migrationBuilder.DropForeignKey(
                name: "FK_Wishlist_Users_UserId",
                table: "Wishlist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wishlist",
                table: "Wishlist");

            migrationBuilder.RenameTable(
                name: "Wishlist",
                newName: "WishlistItems");

            migrationBuilder.RenameIndex(
                name: "IX_Wishlist_UserId",
                table: "WishlistItems",
                newName: "IX_WishlistItems_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Wishlist_GameId",
                table: "WishlistItems",
                newName: "IX_WishlistItems_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WishlistItems",
                table: "WishlistItems",
                column: "WishlistId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Game_GameId",
                table: "WishlistItems",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Users_UserId",
                table: "WishlistItems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
