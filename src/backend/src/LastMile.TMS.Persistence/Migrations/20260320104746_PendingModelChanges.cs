using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Parcel_ZoneId",
                table: "Parcel",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcel_Zones_ZoneId",
                table: "Parcel",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcel_Zones_ZoneId",
                table: "Parcel");

            migrationBuilder.DropIndex(
                name: "IX_Parcel_ZoneId",
                table: "Parcel");
        }
    }
}
