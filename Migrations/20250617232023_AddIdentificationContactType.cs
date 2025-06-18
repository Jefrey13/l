using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentificationContactType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdType",
                schema: "auth",
                table: "ContactLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                schema: "auth",
                table: "ContactLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceCard",
                schema: "auth",
                table: "ContactLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdType",
                schema: "auth",
                table: "ContactLogs");

            migrationBuilder.DropColumn(
                name: "Password",
                schema: "auth",
                table: "ContactLogs");

            migrationBuilder.DropColumn(
                name: "ResidenceCard",
                schema: "auth",
                table: "ContactLogs");
        }
    }
}
