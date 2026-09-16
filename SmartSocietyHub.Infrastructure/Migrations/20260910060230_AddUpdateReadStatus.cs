using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSocietyHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateReadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpdateReadStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateReadStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpdateReadStatuses_Updates_UpdateId",
                        column: x => x.UpdateId,
                        principalTable: "Updates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UpdateReadStatuses_UpdateId_UserId",
                table: "UpdateReadStatuses",
                columns: new[] { "UpdateId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpdateReadStatuses");
        }
    }
}
