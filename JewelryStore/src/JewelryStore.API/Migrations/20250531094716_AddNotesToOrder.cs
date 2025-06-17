using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryStore.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Orders");
        }
    }
}
