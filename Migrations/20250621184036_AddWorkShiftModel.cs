using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkShiftModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkShift_User",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpeningHourId = table.Column<int>(type: "int", nullable: false),
                    AssingedUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkShift_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningHour_Workshift",
                        column: x => x.OpeningHourId,
                        principalSchema: "crm",
                        principalTable: "OpeningHour",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkShift_User_AssignedBy",
                        column: x => x.AssingedUserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkShift_User_CreatedBy",
                        column: x => x.CreatedById,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workshift_User_UpdatedBy",
                        column: x => x.UpdatedById,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkShift_User_AssingedUserId",
                schema: "crm",
                table: "WorkShift_User",
                column: "AssingedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShift_User_CreatedById",
                schema: "crm",
                table: "WorkShift_User",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShift_User_OpeningHourId",
                schema: "crm",
                table: "WorkShift_User",
                column: "OpeningHourId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShift_User_UpdatedById",
                schema: "crm",
                table: "WorkShift_User",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkShift_User",
                schema: "crm");
        }
    }
}
