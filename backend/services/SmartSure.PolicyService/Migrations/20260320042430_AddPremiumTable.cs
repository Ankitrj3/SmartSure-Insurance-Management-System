using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Premiums",
                columns: table => new
                {
                    PremiumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Premiums", x => x.PremiumId);
                    table.ForeignKey(
                        name: "FK_Premiums_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "PolicyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Premiums_PolicyId",
                table: "Premiums",
                column: "PolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Premiums");
        }
    }
}
