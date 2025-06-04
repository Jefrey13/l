using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeColum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "auth",
                table: "SystemParams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "auth",
                table: "SystemParams");
        }
    }
}
