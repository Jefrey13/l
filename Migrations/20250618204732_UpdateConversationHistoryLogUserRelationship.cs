using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationHistoryLogUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationHistoryLog_Users_ChangedByUserId",
                schema: "chat",
                table: "ConversationHistoryLog");

            migrationBuilder.AlterColumn<int>(
                name: "ChangedByUserId",
                schema: "chat",
                table: "ConversationHistoryLog",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationHistoryLog_ChangedByUserId",
                schema: "chat",
                table: "ConversationHistoryLog",
                column: "ChangedByUserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationHistoryLog_ChangedByUserId",
                schema: "chat",
                table: "ConversationHistoryLog");

            migrationBuilder.AlterColumn<int>(
                name: "ChangedByUserId",
                schema: "chat",
                table: "ConversationHistoryLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationHistoryLog_Users_ChangedByUserId",
                schema: "chat",
                table: "ConversationHistoryLog",
                column: "ChangedByUserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
