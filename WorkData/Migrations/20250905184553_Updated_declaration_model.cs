using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkData.Migrations
{
    /// <inheritdoc />
    public partial class Updated_declaration_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "HolidayHours",
                table: "Declarations",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PermissionHours",
                table: "Declarations",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SickHours",
                table: "Declarations",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolidayHours",
                table: "Declarations");

            migrationBuilder.DropColumn(
                name: "PermissionHours",
                table: "Declarations");

            migrationBuilder.DropColumn(
                name: "SickHours",
                table: "Declarations");
        }
    }
}
