using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationControlFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "chat",
                table: "Conversations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "New",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "Bot");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "SYSUTCDATETIME()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "chat",
                table: "Conversations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Bot",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "New");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                schema: "chat",
                table: "Conversations",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
