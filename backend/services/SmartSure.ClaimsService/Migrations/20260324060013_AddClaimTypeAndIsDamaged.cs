using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.ClaimsService.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimTypeAndIsDamaged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaimType",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompletelyDamaged",
                table: "Claims",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimType",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IsCompletelyDamaged",
                table: "Claims");
        }
    }
}
