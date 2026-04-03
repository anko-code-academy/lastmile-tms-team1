using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackingEvent_Parcel_ParcelId",
                table: "TrackingEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackingEvent",
                table: "TrackingEvent");

            migrationBuilder.RenameTable(
                name: "TrackingEvent",
                newName: "TrackingEvents");

            migrationBuilder.RenameIndex(
                name: "IX_TrackingEvent_Timestamp",
                table: "TrackingEvents",
                newName: "IX_TrackingEvents_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_TrackingEvent_ParcelId_Timestamp",
                table: "TrackingEvents",
                newName: "IX_TrackingEvents_ParcelId_Timestamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackingEvents",
                table: "TrackingEvents",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ParcelAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelAuditLogs_Parcel_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAuditLogs_ParcelId",
                table: "ParcelAuditLogs",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAuditLogs_ParcelId_CreatedAt",
                table: "ParcelAuditLogs",
                columns: new[] { "ParcelId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrackingEvents_Parcel_ParcelId",
                table: "TrackingEvents",
                column: "ParcelId",
                principalTable: "Parcel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackingEvents_Parcel_ParcelId",
                table: "TrackingEvents");

            migrationBuilder.DropTable(
                name: "ParcelAuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackingEvents",
                table: "TrackingEvents");

            migrationBuilder.RenameTable(
                name: "TrackingEvents",
                newName: "TrackingEvent");

            migrationBuilder.RenameIndex(
                name: "IX_TrackingEvents_Timestamp",
                table: "TrackingEvent",
                newName: "IX_TrackingEvent_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_TrackingEvents_ParcelId_Timestamp",
                table: "TrackingEvent",
                newName: "IX_TrackingEvent_ParcelId_Timestamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackingEvent",
                table: "TrackingEvent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackingEvent_Parcel_ParcelId",
                table: "TrackingEvent",
                column: "ParcelId",
                principalTable: "Parcel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
