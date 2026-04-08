using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverIdToRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DriverId",
                table: "Routes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DriverId",
                table: "Routes",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Drivers_DriverId",
                table: "Routes",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Drivers_DriverId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_DriverId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Routes");
        }
    }
}
