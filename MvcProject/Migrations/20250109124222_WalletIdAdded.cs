using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcProject.Migrations
{
    /// <inheritdoc />
    public partial class WalletIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "AspNetUsers");
        }
    }
}
