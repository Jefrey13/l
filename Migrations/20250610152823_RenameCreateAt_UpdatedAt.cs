using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreateAt_UpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentComment",
                schema: "chat",
                table: "Conversations",
                type: "nvarchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentResponseAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentComment",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AssignmentResponseAt",
                schema: "chat",
                table: "Conversations");
        }
    }
}
