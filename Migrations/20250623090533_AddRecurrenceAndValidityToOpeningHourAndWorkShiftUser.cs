using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurrenceAndValidityToOpeningHourAndWorkShiftUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHour_CreateBy",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHour_UpdateBy",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHour_Workshift",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkShift_User_AssignedBy",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshift_User_UpdatedBy",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropColumn(
                name: "IsHoliday",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.RenameColumn(
                name: "AssingedUserId",
                schema: "crm",
                table: "WorkShift_User",
                newName: "AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkShift_User_AssingedUserId",
                schema: "crm",
                table: "WorkShift_User",
                newName: "IX_WorkShift_User_AssignedUserId");

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedById",
                schema: "crm",
                table: "WorkShift_User",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DropColumn(
            name: "RowVersion",
            schema: "crm",
            table: "WorkShift_User");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "crm",
                table: "WorkShift_User",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "crm",
                table: "WorkShift_User",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidFrom",
                schema: "crm",
                table: "WorkShift_User",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidTo",
                schema: "crm",
                table: "WorkShift_User",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                schema: "crm",
                table: "OpeningHour",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "crm",
                table: "OpeningHour",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                schema: "crm",
                table: "OpeningHour",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                schema: "crm",
                table: "OpeningHour",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "crm",
                table: "OpeningHour",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                schema: "crm",
                table: "OpeningHour",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recurrence",
                schema: "crm",
                table: "OpeningHour",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "SpecificDate",
                schema: "crm",
                table: "OpeningHour",
                type: "date",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHour_CreatedBy",
                schema: "crm",
                table: "OpeningHour",
                column: "CreatedById",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHour_UpdatedBy",
                schema: "crm",
                table: "OpeningHour",
                column: "UpdatedById",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHour_WorkShift_User",
                schema: "crm",
                table: "WorkShift_User",
                column: "OpeningHourId",
                principalSchema: "crm",
                principalTable: "OpeningHour",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkShift_User_AssignedUser",
                schema: "crm",
                table: "WorkShift_User",
                column: "AssignedUserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkShift_User_UpdatedBy",
                schema: "crm",
                table: "WorkShift_User",
                column: "UpdatedById",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHour_CreatedBy",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHour_UpdatedBy",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHour_WorkShift_User",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkShift_User_AssignedUser",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkShift_User_UpdatedBy",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                schema: "crm",
                table: "WorkShift_User");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropColumn(
                name: "Recurrence",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.DropColumn(
                name: "SpecificDate",
                schema: "crm",
                table: "OpeningHour");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                schema: "crm",
                table: "WorkShift_User",
                newName: "AssingedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkShift_User_AssignedUserId",
                schema: "crm",
                table: "WorkShift_User",
                newName: "IX_WorkShift_User_AssingedUserId");

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedById",
                schema: "crm",
                table: "WorkShift_User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
            name: "RowVersion",
            schema: "crm",
            table: "WorkShift_User");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "crm",
                table: "WorkShift_User",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "crm",
                table: "WorkShift_User",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                schema: "crm",
                table: "OpeningHour",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "crm",
                table: "OpeningHour",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                schema: "crm",
                table: "OpeningHour",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AddColumn<bool>(
                name: "IsHoliday",
                schema: "crm",
                table: "OpeningHour",
                type: "bit",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHour_CreateBy",
                schema: "crm",
                table: "OpeningHour",
                column: "CreatedById",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHour_UpdateBy",
                schema: "crm",
                table: "OpeningHour",
                column: "UpdatedById",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHour_Workshift",
                schema: "crm",
                table: "WorkShift_User",
                column: "OpeningHourId",
                principalSchema: "crm",
                principalTable: "OpeningHour",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkShift_User_AssignedBy",
                schema: "crm",
                table: "WorkShift_User",
                column: "AssingedUserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshift_User_UpdatedBy",
                schema: "crm",
                table: "WorkShift_User",
                column: "UpdatedById",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
