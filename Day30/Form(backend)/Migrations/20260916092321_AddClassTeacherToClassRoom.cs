using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Form.Migrations
{
    /// <inheritdoc />
    public partial class AddClassTeacherToClassRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassTeacherUserId",
                table: "ClassRooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassRooms_ClassTeacherUserId",
                table: "ClassRooms",
                column: "ClassTeacherUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRooms_Users_ClassTeacherUserId",
                table: "ClassRooms",
                column: "ClassTeacherUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRooms_Users_ClassTeacherUserId",
                table: "ClassRooms");

            migrationBuilder.DropIndex(
                name: "IX_ClassRooms_ClassTeacherUserId",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "ClassTeacherUserId",
                table: "ClassRooms");
        }
    }
}
