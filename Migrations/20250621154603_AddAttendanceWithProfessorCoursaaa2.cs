using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceWithProfessorCoursaaa2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "MaxAbsenceLimitPractical",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAbsenceLimitTheoretical",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "fullAttendance",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "MaxAbsenceLimitPractical",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "MaxAbsenceLimitTheoretical",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "fullAttendance",
                table: "Courses");

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
    }
}
