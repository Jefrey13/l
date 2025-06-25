using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayMovedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isWorkShift",
                schema: "crm",
                table: "OpeningHour",
                newName: "IsWorkShift");

            migrationBuilder.AddColumn<DateOnly>(
                name: "HolidayMovedFrom",
                schema: "crm",
                table: "OpeningHour",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHolidayMoved",
                schema: "crm",
                table: "OpeningHour",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolidayMovedFrom",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropColumn(
                name: "IsHolidayMoved",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.RenameColumn(
                name: "IsWorkShift",
                schema: "crm",
                table: "OpeningHour",
                newName: "isWorkShift");
        }
    }
}
