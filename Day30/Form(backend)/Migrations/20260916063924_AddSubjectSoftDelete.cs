using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Form.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_ClassRoomId",
                table: "Subjects");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Archieved",
                table: "Subjects",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Subjects",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Subjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ClassRoomId_Name",
                table: "Subjects",
                columns: new[] { "ClassRoomId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_ClassRoomId_Name",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Archieved",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Subjects");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ClassRoomId",
                table: "Subjects",
                column: "ClassRoomId");
        }
    }
}
