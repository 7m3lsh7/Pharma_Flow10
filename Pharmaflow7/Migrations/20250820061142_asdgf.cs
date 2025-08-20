using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmaflow7.Migrations
{
    /// <inheritdoc />
    public partial class asdgf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DestinationLongitude",
                table: "Shipments",
                type: "decimal(18,9)",
                precision: 18,
                scale: 9,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DestinationLatitude",
                table: "Shipments",
                type: "decimal(18,9)",
                precision: 18,
                scale: 9,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DestinationLongitude",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)",
                oldPrecision: 18,
                oldScale: 9,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DestinationLatitude",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)",
                oldPrecision: 18,
                oldScale: 9,
                oldNullable: true);
        }
    }
}
