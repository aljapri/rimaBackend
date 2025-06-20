using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceWithProfessorCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0729ad92-6450-4eb5-ba7e-a7331c59590e"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("3d74bd3b-1def-4e2c-a6a3-8f15429ae770"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("dfbeb566-bbe5-4586-bc68-ecb6aec7dfe8"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProfessorCourseId",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("1924ca37-d38a-4bbc-ae1d-5a7f0cf4618a"), null, "Admin", "ADMIN" },
                    { new Guid("1f3e412a-1ada-4aa4-b50a-efd8feef26df"), null, "Student", "STUDENT" },
                    { new Guid("8a16fd55-9ee0-4842-9a38-849cb0c21e97"), null, "Professor", "PROFESSOR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ProfessorCourseId",
                table: "Attendances",
                column: "ProfessorCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_ProfessorCourses_ProfessorCourseId",
                table: "Attendances",
                column: "ProfessorCourseId",
                principalTable: "ProfessorCourses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_ProfessorCourses_ProfessorCourseId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ProfessorCourseId",
                table: "Attendances");

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

            migrationBuilder.DropColumn(
                name: "ProfessorCourseId",
                table: "Attendances");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0729ad92-6450-4eb5-ba7e-a7331c59590e"), null, "Admin", "ADMIN" },
                    { new Guid("3d74bd3b-1def-4e2c-a6a3-8f15429ae770"), null, "Student", "STUDENT" },
                    { new Guid("dfbeb566-bbe5-4586-bc68-ecb6aec7dfe8"), null, "Professor", "PROFESSOR" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
