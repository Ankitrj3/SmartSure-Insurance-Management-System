using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeDetailAreaAndCostFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AreaSqFt",
                table: "HomeDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConstructionCostPerSqFt",
                table: "HomeDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaSqFt",
                table: "HomeDetails");

            migrationBuilder.DropColumn(
                name: "ConstructionCostPerSqFt",
                table: "HomeDetails");
        }
    }
}
