using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class AverageAgentResponseTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotificationId",
                schema: "chat",
                table: "Notifications",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "chat",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "AverageAgentResponseTime",
                schema: "chat",
                table: "Conversations",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageAgentResponseTime",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "chat",
                table: "Notifications",
                newName: "NotificationId");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                schema: "chat",
                table: "Notifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
