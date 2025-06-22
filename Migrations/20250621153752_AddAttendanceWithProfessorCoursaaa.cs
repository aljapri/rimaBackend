using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceWithProfessorCoursaaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1924ca37-d38a-4bbc-ae1d-5a7f0cf4618a"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1f3e412a-1ada-4aa4-b50a-efd8feef26df"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8a16fd55-9ee0-4842-9a38-849cb0c21e97"));

            migrationBuilder.AddColumn<int>(
                name: "fullAttendance",
                table: "ProfessorCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("050b7e13-0288-4efd-b769-83860564700f"), null, "Admin", "ADMIN" },
                    { new Guid("1ddbe30e-f7b6-46c6-9ac6-354700608abb"), null, "Professor", "PROFESSOR" },
                    { new Guid("5e433dc4-b3d9-4278-830c-016f69231d54"), null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("050b7e13-0288-4efd-b769-83860564700f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1ddbe30e-f7b6-46c6-9ac6-354700608abb"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("5e433dc4-b3d9-4278-830c-016f69231d54"));

            migrationBuilder.DropColumn(
                name: "fullAttendance",
                table: "ProfessorCourses");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("1924ca37-d38a-4bbc-ae1d-5a7f0cf4618a"), null, "Admin", "ADMIN" },
                    { new Guid("1f3e412a-1ada-4aa4-b50a-efd8feef26df"), null, "Student", "STUDENT" },
                    { new Guid("8a16fd55-9ee0-4842-9a38-849cb0c21e97"), null, "Professor", "PROFESSOR" }
                });
        }
    }
}
