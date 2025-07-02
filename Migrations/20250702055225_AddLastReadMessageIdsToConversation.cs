using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReadMessageIdsToConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentLastReadMessageId",
                schema: "chat",
                table: "Conversations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignerLastReadMessageId",
                schema: "chat",
                table: "Conversations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentLastReadMessageId",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AssignerLastReadMessageId",
                schema: "chat",
                table: "Conversations");
        }
    }
}
