using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSocietyHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePropertyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Properties_HouseNumber_Block",
                table: "Properties",
                columns: new[] { "HouseNumber", "Block" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Properties_HouseNumber_Block",
                table: "Properties");
        }
    }
}
