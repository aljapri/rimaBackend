using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceWithProfessorCoursaaa21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("63afb3ac-5ba0-4ee7-91aa-52cb38212412"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("75c9fd8c-a414-48d4-835b-945c1b9a1642"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("da19b9c8-04c7-417e-ba76-c62f78910662"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("a203d2d5-cb94-47e0-a192-85cee9d4ca09"), null, "Admin", "ADMIN" },
                    { new Guid("a86edea9-5fcf-4bd6-866c-d1f92b121cfe"), null, "Student", "STUDENT" },
                    { new Guid("c79c89d1-ca65-47ae-b2ae-8ddabb42ab1a"), null, "Professor", "PROFESSOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a203d2d5-cb94-47e0-a192-85cee9d4ca09"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a86edea9-5fcf-4bd6-866c-d1f92b121cfe"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c79c89d1-ca65-47ae-b2ae-8ddabb42ab1a"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("63afb3ac-5ba0-4ee7-91aa-52cb38212412"), null, "Student", "STUDENT" },
                    { new Guid("75c9fd8c-a414-48d4-835b-945c1b9a1642"), null, "Professor", "PROFESSOR" },
                    { new Guid("da19b9c8-04c7-417e-ba76-c62f78910662"), null, "Admin", "ADMIN" }
                });
        }
    }
}
