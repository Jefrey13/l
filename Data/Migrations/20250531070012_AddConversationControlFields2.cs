using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationControlFields2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AgentFirstMessageAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgentLastMessageAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClientLastMessageAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAgentAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarningSentAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentFirstMessageAt",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AgentLastMessageAt",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ClientLastMessageAt",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RequestedAgentAt",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "WarningSentAt",
                schema: "chat",
                table: "Conversations");
        }
    }
}
