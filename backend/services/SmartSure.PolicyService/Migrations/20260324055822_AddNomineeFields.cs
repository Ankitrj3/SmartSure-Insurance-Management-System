using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class AddNomineeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomineeName",
                table: "Policies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineeRelation",
                table: "Policies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomineeName",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "NomineeRelation",
                table: "Policies");
        }
    }
}
