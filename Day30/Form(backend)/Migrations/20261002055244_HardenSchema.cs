using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Form.Migrations
{
    /// <inheritdoc />
    public partial class HardenSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_EnrollmentId_Date",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Archieved",
                table: "Subjects");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "PasswordResetTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "PasswordResetTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ClassRooms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassRooms_Name_AcademicYear",
                table: "ClassRooms",
                columns: new[] { "Name", "AcademicYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EnrollmentId_Date",
                table: "AttendanceRecords",
                columns: new[] { "EnrollmentId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ClassRooms_Name_AcademicYear",
                table: "ClassRooms");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_EnrollmentId_Date",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "PasswordResetTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Archieved",
                table: "Subjects",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "PasswordResetTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ClassRooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EnrollmentId_Date",
                table: "AttendanceRecords",
                columns: new[] { "EnrollmentId", "Date" });
        }
    }
}
