using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Form.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Enrollments");
        }
    }
}
