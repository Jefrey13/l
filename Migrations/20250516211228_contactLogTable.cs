using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class contactLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Client",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "chat",
                table: "Conversations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContactLogs",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdCard = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactLogs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "crm",
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId",
                schema: "chat",
                table: "Conversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogs_CompanyId",
                schema: "auth",
                table: "ContactLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "UQ_ContactLog_Phine",
                schema: "auth",
                table: "ContactLogs",
                column: "Phone",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_UserId",
                schema: "chat",
                table: "Conversations",
                column: "UserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "Fk_Conversations_Client",
                schema: "chat",
                table: "Conversations",
                column: "ClientUserId",
                principalSchema: "auth",
                principalTable: "ContactLogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_UserId",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "Fk_Conversations_Client",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "ContactLogs",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserId",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "chat",
                table: "Conversations");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Client",
                schema: "chat",
                table: "Conversations",
                column: "ClientUserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
