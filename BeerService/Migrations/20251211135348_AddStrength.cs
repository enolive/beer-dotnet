using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerService.Migrations
{
    /// <inheritdoc />
    public partial class AddStrength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Strength",
                table: "Beers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Strength",
                table: "Beers");
        }
    }
}
