using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipVerifyContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                schema: "auth",
                table: "ContactLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                schema: "auth",
                table: "ContactLogs",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<int>(
                name: "VerifiedId",
                schema: "auth",
                table: "ContactLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogs_VerifiedId",
                schema: "auth",
                table: "ContactLogs",
                column: "VerifiedId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactLogs_verifyUser",
                schema: "auth",
                table: "ContactLogs",
                column: "VerifiedId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactLogs_verifyUser",
                schema: "auth",
                table: "ContactLogs");

            migrationBuilder.DropIndex(
                name: "IX_ContactLogs_VerifiedId",
                schema: "auth",
                table: "ContactLogs");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                schema: "auth",
                table: "ContactLogs");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                schema: "auth",
                table: "ContactLogs");

            migrationBuilder.DropColumn(
                name: "VerifiedId",
                schema: "auth",
                table: "ContactLogs");
        }
    }
}
