using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InsuranceTypes",
                columns: new[] { "TypeId", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Comprehensive vehicle insurance coverage", "Vehicle" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Complete home and property insurance", "Home" }
                });

            migrationBuilder.InsertData(
                table: "InsuranceSubtypes",
                columns: new[] { "SubtypeId", "BasePremium", "Description", "Name", "TypeId" },
                values: new object[,]
                {
                    { new Guid("11111111-0001-0000-0000-000000000001"), 15000m, "Insurance for Mahindra vehicles", "Mahindra", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0002-0000-0000-000000000002"), 12000m, "Insurance for Maruti Suzuki vehicles", "Maruti Suzuki", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0003-0000-0000-000000000003"), 13000m, "Insurance for Hyundai vehicles", "Hyundai", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0004-0000-0000-000000000004"), 14000m, "Insurance for Honda vehicles", "Honda", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0005-0000-0000-000000000005"), 13500m, "Insurance for Tata vehicles", "Tata Motors", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0006-0000-0000-000000000006"), 16000m, "Insurance for Toyota vehicles", "Toyota", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0007-0000-0000-000000000007"), 14500m, "Insurance for Kia vehicles", "Kia", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0008-0000-0000-000000000008"), 17000m, "Insurance for Volkswagen vehicles", "Volkswagen", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0009-0000-0000-000000000009"), 16500m, "Insurance for Skoda vehicles", "Skoda", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0010-0000-0000-000000000010"), 13000m, "Insurance for Renault vehicles", "Renault", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0011-0000-0000-000000000011"), 14000m, "Insurance for Nissan vehicles", "Nissan", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0012-0000-0000-000000000012"), 15500m, "Insurance for Ford vehicles", "Ford", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0013-0000-0000-000000000013"), 15000m, "Insurance for MG Motor vehicles", "MG Motor", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0014-0000-0000-000000000014"), 18000m, "Insurance for Jeep vehicles", "Jeep", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0015-0000-0000-000000000015"), 25000m, "Insurance for BMW vehicles", "BMW", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0016-0000-0000-000000000016"), 28000m, "Insurance for Mercedes-Benz vehicles", "Mercedes-Benz", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0017-0000-0000-000000000017"), 26000m, "Insurance for Audi vehicles", "Audi", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-0018-0000-0000-000000000018"), 24000m, "Insurance for Volvo vehicles", "Volvo", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("22222222-0001-0000-0000-000000000001"), 8000m, "Insurance for apartments and flats", "Apartment", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0002-0000-0000-000000000002"), 12000m, "Insurance for independent houses", "Independent House", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0003-0000-0000-000000000003"), 18000m, "Insurance for villas and luxury homes", "Villa", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0004-0000-0000-000000000004"), 15000m, "Insurance for bungalows", "Bungalow", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0005-0000-0000-000000000005"), 20000m, "Insurance for penthouses", "Penthouse", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0006-0000-0000-000000000006"), 6000m, "Insurance for studio apartments", "Studio Apartment", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0007-0000-0000-000000000007"), 14000m, "Insurance for duplex homes", "Duplex", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-0008-0000-0000-000000000008"), 16000m, "Insurance for farmhouses", "Farmhouse", new Guid("22222222-2222-2222-2222-222222222222") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0001-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0002-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0003-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0004-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0005-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0006-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0007-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0008-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0009-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0010-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0011-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0012-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0013-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0014-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0015-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0016-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0017-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("11111111-0018-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0001-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0002-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0003-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0004-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0005-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0006-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0007-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "InsuranceSubtypes",
                keyColumn: "SubtypeId",
                keyValue: new Guid("22222222-0008-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "InsuranceTypes",
                keyColumn: "TypeId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "InsuranceTypes",
                keyColumn: "TypeId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
