using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class newHolidayProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationHistoryLog_Conversations_ConversationId",
                schema: "chat",
                table: "ConversationHistoryLog");

            migrationBuilder.AddColumn<DateOnly>(
                name: "HolidayDate",
                schema: "crm",
                table: "OpeningHour",
                type: "date",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationHistoryLog_Conversation",
                schema: "chat",
                table: "ConversationHistoryLog",
                column: "ConversationId",
                principalSchema: "chat",
                principalTable: "Conversations",
                principalColumn: "ConversationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationHistoryLog_Conversation",
                schema: "chat",
                table: "ConversationHistoryLog");

            migrationBuilder.DropColumn(
                name: "HolidayDate",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationHistoryLog_Conversations_ConversationId",
                schema: "chat",
                table: "ConversationHistoryLog",
                column: "ConversationId",
                principalSchema: "chat",
                principalTable: "Conversations",
                principalColumn: "ConversationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
