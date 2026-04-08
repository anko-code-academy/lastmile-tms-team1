using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteStopAndRouteZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ZoneId",
                table: "Routes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RouteStopId",
                table: "Parcel",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedServiceMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AccessInstructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Street1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GeoLocation = table.Column<Point>(type: "geometry (point)", nullable: true),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteStops_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ZoneId",
                table: "Routes",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcel_RouteStopId",
                table: "Parcel",
                column: "RouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_GeoLocation",
                table: "RouteStops",
                column: "GeoLocation");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteId_SequenceNumber",
                table: "RouteStops",
                columns: new[] { "RouteId", "SequenceNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parcel_RouteStops_RouteStopId",
                table: "Parcel",
                column: "RouteStopId",
                principalTable: "RouteStops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Zones_ZoneId",
                table: "Routes",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcel_RouteStops_RouteStopId",
                table: "Parcel");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Zones_ZoneId",
                table: "Routes");

            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropIndex(
                name: "IX_Routes_ZoneId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Parcel_RouteStopId",
                table: "Parcel");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "RouteStopId",
                table: "Parcel");
        }
    }
}
