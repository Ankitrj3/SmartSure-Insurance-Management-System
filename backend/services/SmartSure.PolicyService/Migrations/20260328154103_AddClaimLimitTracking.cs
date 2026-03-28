using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimLimitTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedClaimsCount",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTerminated",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedClaimsCount",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "IsTerminated",
                table: "Policies");
        }
    }
}
