using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitParcelSearchVectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Parcel_SearchVector",
                table: "Parcel");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_SearchVector",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Parcel");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Addresses");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "AddressSearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"Street1\", '') || ' ' || coalesce(\"City\", '') || ' ' || coalesce(\"PostalCode\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "RecipientNameSearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"ContactName\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressSearchVector",
                table: "Addresses",
                column: "AddressSearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_RecipientNameSearchVector",
                table: "Addresses",
                column: "RecipientNameSearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_AddressSearchVector",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_RecipientNameSearchVector",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "AddressSearchVector",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "RecipientNameSearchVector",
                table: "Addresses");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Parcel",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"TrackingNumber\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"ContactName\", '') || ' ' || coalesce(\"Street1\", '') || ' ' || coalesce(\"City\", '') || ' ' || coalesce(\"PostalCode\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parcel_SearchVector",
                table: "Parcel",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_SearchVector",
                table: "Addresses",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }
    }
}
