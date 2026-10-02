using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Form.Migrations
{
    /// <inheritdoc />
    public partial class deleteclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ClassRooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClassRooms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClassRooms");
        }
    }
}
