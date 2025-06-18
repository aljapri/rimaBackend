using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace kalamon_University.Migrations
{
    /// <inheritdoc />
    public partial class YourMigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0a0ac982-ed81-4144-a937-d92a57d726fb"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("72700355-973f-45a4-8267-65d81c9e3f7d"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7a6ee892-bc91-45be-97dd-27f75ceb5288"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("4dd88fe6-d707-4383-900e-93b786ad127c"), null, "Professor", "PROFESSOR" },
                    { new Guid("8910548c-dff6-40f9-93a2-93e6fbff7c7c"), null, "Admin", "ADMIN" },
                    { new Guid("e7bea3fb-3d78-450a-8c7e-92efe9131573"), null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("4dd88fe6-d707-4383-900e-93b786ad127c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8910548c-dff6-40f9-93a2-93e6fbff7c7c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e7bea3fb-3d78-450a-8c7e-92efe9131573"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0a0ac982-ed81-4144-a937-d92a57d726fb"), null, "Student", "STUDENT" },
                    { new Guid("72700355-973f-45a4-8267-65d81c9e3f7d"), null, "Professor", "PROFESSOR" },
                    { new Guid("7a6ee892-bc91-45be-97dd-27f75ceb5288"), null, "Admin", "ADMIN" }
                });
        }
    }
}
