using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations;

/// <inheritdoc />
public partial class RedesignRouteStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename Planned → Draft
        migrationBuilder.Sql(
            @"UPDATE ""Routes"" SET ""Status"" = 'Draft' WHERE ""Status"" = 'Planned'");

        // Cancelled routes become Completed (closest terminal state)
        migrationBuilder.Sql(
            @"UPDATE ""Routes"" SET ""Status"" = 'Completed' WHERE ""Status"" = 'Cancelled'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert Draft → Planned
        migrationBuilder.Sql(
            @"UPDATE ""Routes"" SET ""Status"" = 'Planned' WHERE ""Status"" = 'Draft'");
    }
}
