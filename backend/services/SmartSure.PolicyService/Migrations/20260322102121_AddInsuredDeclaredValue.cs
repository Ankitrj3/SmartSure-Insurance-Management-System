using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuredDeclaredValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InsuredDeclaredValue",
                table: "Policies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuredDeclaredValue",
                table: "Policies");
        }
    }
}
