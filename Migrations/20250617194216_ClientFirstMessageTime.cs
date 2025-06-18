using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class ClientFirstMessageTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IncompletedAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncompletedAt",
                schema: "chat",
                table: "Conversations");
        }
    }
}
