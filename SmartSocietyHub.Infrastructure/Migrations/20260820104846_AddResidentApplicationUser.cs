using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSocietyHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Residents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Residents_ApplicationUserId",
                table: "Residents",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Residents_AspNetUsers_ApplicationUserId",
                table: "Residents",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residents_AspNetUsers_ApplicationUserId",
                table: "Residents");

            migrationBuilder.DropIndex(
                name: "IX_Residents_ApplicationUserId",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Residents");
        }
    }
}
