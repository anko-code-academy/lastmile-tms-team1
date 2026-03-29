using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOperatingHoursJsonWithShiftSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Make DriverId nullable (before adding DepotId FK)
            migrationBuilder.AlterColumn<Guid>(
                name: "DriverId",
                table: "ShiftSchedules",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Step 2: Add DepotId column (nullable)
            migrationBuilder.AddColumn<Guid>(
                name: "DepotId",
                table: "ShiftSchedules",
                type: "uuid",
                nullable: true);

            // Step 3: Drop the old unique index (will be replaced by partial indexes)
            migrationBuilder.DropIndex(
                name: "IX_ShiftSchedules_DriverId_DayOfWeek",
                table: "ShiftSchedules");

            // Step 4: Copy data from Depots.OperatingHoursJson to ShiftSchedules
            // Each JSON entry becomes a ShiftSchedule row with DepotId set, DriverId null
            migrationBuilder.Sql(@"
                INSERT INTO ""ShiftSchedules"" (""Id"", ""DayOfWeek"", ""OpenTime"", ""CloseTime"", ""DriverId"", ""DepotId"", ""IsDeleted"", ""CreatedAt"", ""CreatedBy"", ""LastModifiedAt"", ""LastModifiedBy"", ""DeletedAt"", ""DeletedBy"")
                SELECT
                    gen_random_uuid(),
                    ((json_array_elements->>'DayOfWeek')::text)::integer,
                    ((json_array_elements->>'OpenTime')::text)::time,
                    ((json_array_elements->>'CloseTime')::text)::time,
                    NULL::uuid,
                    d.""Id"",
                    false,
                    NOW(),
                    NULL::text,
                    NOW(),
                    NULL::text,
                    NULL::timestamptz,
                    NULL::text
                FROM ""Depots"" d
                CROSS JOIN json_array_elements(d.""OperatingHoursJson""::json) AS json_array_elements
                WHERE d.""OperatingHoursJson"" IS NOT NULL;
            ");

            // Step 5: Drop the OperatingHoursJson column
            migrationBuilder.DropColumn(
                name: "OperatingHoursJson",
                table: "Depots");

            // Step 6: Add FK from ShiftSchedules to Depots (DepotId)
            migrationBuilder.AddForeignKey(
                name: "FK_ShiftSchedules_Depots_DepotId",
                table: "ShiftSchedules",
                column: "DepotId",
                principalTable: "Depots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Step 7: Add XOR check constraint (exactly one of DriverId/DepotId must be set)
            migrationBuilder.Sql(@"
                ALTER TABLE ""ShiftSchedules""
                ADD CONSTRAINT ""CK_ShiftSchedules_Xor_FK""
                CHECK ((""DriverId"" IS NOT NULL) <> (""DepotId"" IS NOT NULL));
            ");

            // Step 8: Create partial unique index for DriverId + DayOfWeek (DriverId IS NOT NULL)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""ix_shift_schedules_driver_day""
                ON ""ShiftSchedules"" (""DriverId"", ""DayOfWeek"")
                WHERE ""DriverId"" IS NOT NULL;
            ");

            // Step 9: Create partial unique index for DepotId + DayOfWeek (DepotId IS NOT NULL)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""ix_shift_schedules_depot_day""
                ON ""ShiftSchedules"" (""DepotId"", ""DayOfWeek"")
                WHERE ""DepotId"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop partial unique indexes
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_shift_schedules_depot_day"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_shift_schedules_driver_day"";");

            // Drop XOR check constraint
            migrationBuilder.Sql(@"ALTER TABLE ""ShiftSchedules"" DROP CONSTRAINT ""CK_ShiftSchedules_Xor_FK"";");

            // Drop FK to Depots
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftSchedules_Depots_DepotId",
                table: "ShiftSchedules");

            // Add back OperatingHoursJson column
            migrationBuilder.AddColumn<string>(
                name: "OperatingHoursJson",
                table: "Depots",
                type: "text",
                nullable: true);

            // Restore the unique index on DriverId + DayOfWeek
            migrationBuilder.CreateIndex(
                name: "IX_ShiftSchedules_DriverId_DayOfWeek",
                table: "ShiftSchedules",
                columns: new[] { "DriverId", "DayOfWeek" },
                unique: true);

            // Remove the copied ShiftSchedule rows (those with DepotId set)
            migrationBuilder.Sql(@"
                DELETE FROM ""ShiftSchedules""
                WHERE ""DepotId"" IS NOT NULL;
            ");

            // Set DriverId back to non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "DriverId",
                table: "ShiftSchedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Drop DepotId column
            migrationBuilder.DropColumn(
                name: "DepotId",
                table: "ShiftSchedules");
        }
    }
}