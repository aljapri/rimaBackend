using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace kalamon_University.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDb3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Professors_ProfessorId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ProfessorId",
                table: "Courses");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0ff93b72-b888-40f7-abff-30e9bb26de7b"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("15e67a9b-ee87-4873-aa71-e0e8a9a48474"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e64eb6fb-e7b5-4224-a9b4-09a3f12dd67c"));

            migrationBuilder.DropColumn(
                name: "ProfessorId",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "ProfessorCourses",
                columns: table => new
                {
                    ProfessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessorCourses", x => new { x.ProfessorId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_ProfessorCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfessorCourses_Professors_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Professors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("33208edf-2317-434e-8a29-46b426632058"), null, "Professor", "PROFESSOR" },
                    { new Guid("5be74b8f-f0c2-472f-b8f8-0f1af5e8a34f"), null, "Student", "STUDENT" },
                    { new Guid("98cfdf11-9338-464b-b843-1e3b93bcf899"), null, "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfessorCourses_CourseId",
                table: "ProfessorCourses",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfessorCourses");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("33208edf-2317-434e-8a29-46b426632058"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("5be74b8f-f0c2-472f-b8f8-0f1af5e8a34f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("98cfdf11-9338-464b-b843-1e3b93bcf899"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProfessorId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0ff93b72-b888-40f7-abff-30e9bb26de7b"), null, "Professor", "PROFESSOR" },
                    { new Guid("15e67a9b-ee87-4873-aa71-e0e8a9a48474"), null, "Admin", "ADMIN" },
                    { new Guid("e64eb6fb-e7b5-4224-a9b4-09a3f12dd67c"), null, "Student", "STUDENT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ProfessorId",
                table: "Courses",
                column: "ProfessorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Professors_ProfessorId",
                table: "Courses",
                column: "ProfessorId",
                principalTable: "Professors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
