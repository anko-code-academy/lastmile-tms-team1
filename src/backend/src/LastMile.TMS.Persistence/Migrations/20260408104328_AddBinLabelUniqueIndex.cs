using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBinLabelUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bins_Label",
                table: "Bins");

            migrationBuilder.CreateIndex(
                name: "IX_Bins_Label",
                table: "Bins",
                column: "Label",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bins_Label",
                table: "Bins");

            migrationBuilder.CreateIndex(
                name: "IX_Bins_Label",
                table: "Bins",
                column: "Label");
        }
    }
}
