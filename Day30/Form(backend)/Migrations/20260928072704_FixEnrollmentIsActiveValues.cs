using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Form.Migrations
{
    /// <inheritdoc />
    public partial class FixEnrollmentIsActiveValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Fix existing rows — enrollments should be active if their user is active
            migrationBuilder.Sql(@"
        UPDATE E
        SET 
            E.IsActive = U.IsActive,
            E.DeactivatedAt = CASE 
                WHEN U.IsActive = 0 THEN GETUTCDATE() 
                ELSE NULL 
            END
        FROM Enrollments E
        INNER JOIN Users U ON U.Id = E.StudentUserId;
    ");

            // Step 2: Change the schema default so future inserts are active
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            // Optional: Revert the data fix (set all back to false)
            // Usually NOT needed since Down is rarely called
            // migrationBuilder.Sql("UPDATE Enrollments SET IsActive = 0;");
        }
    }
}
