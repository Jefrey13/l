using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.CreateTable(
                name: "AppRoles",
                schema: "auth",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                schema: "crm",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                schema: "auth",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "chat",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "ContactLogs",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WaId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WaUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdCard = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "New"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactLogs_Companies",
                        column: x => x.CompanyId,
                        principalSchema: "crm",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    DataRequested = table.Column<bool>(type: "bit", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    SecurityStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()")
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()")
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    Identifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    LastOnline = table.Column<DateTime>(type: "datetime2", nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Companies",
                        column: x => x.CompanyId,
                        principalSchema: "crm",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.CreateTable(
                name: "RoleMenus",
                schema: "auth",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenus", x => new { x.MenuId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RoleMenus_Menus",
                        column: x => x.MenuId,
                        principalSchema: "auth",
                        principalTable: "Menus",
                        principalColumn: "MenuId");
                    table.ForeignKey(
                        name: "FK_RoleMenus_Roles",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "AppRoles",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "AuthTokens",
                schema: "auth",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<bool>(type: "bit", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_AuthTokens_Users",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                schema: "chat",
                columns: table => new
                {
                    ConversationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientContactId = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    AssignedAgentId = table.Column<int>(type: "int", nullable: true),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "SYSUTCDATETIME()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Bot"),
                    Initialized = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FirstResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Conversations_AssignedAgent",
                        column: x => x.AssignedAgentId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_AssignedByUser",
                        column: x => x.AssignedByUserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_ClientContact",
                        column: x => x.ClientContactId,
                        principalSchema: "auth",
                        principalTable: "ContactLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                schema: "chat",
                columns: table => new
                {
                    NotificationRecipientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.NotificationRecipientId);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "chat",
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "AppRoles",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "chat",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<int>(type: "int", nullable: true),
                    SenderContactId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InteractiveId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InteractiveTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Text"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Sent"),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.CheckConstraint("CK_Message_OneSender", "(SenderUserId IS NOT NULL AND SenderContactId IS NULL) OR (SenderUserId IS NULL AND SenderContactId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Messages_Conversations",
                        column: x => x.ConversationId,
                        principalSchema: "chat",
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_SenderContact",
                        column: x => x.SenderContactId,
                        principalSchema: "auth",
                        principalTable: "ContactLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_SenderUser",
                        column: x => x.SenderUserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                schema: "chat",
                columns: table => new
                {
                    AttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    MediaId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_Attachments_Messages",
                        column: x => x.MessageId,
                        principalSchema: "chat",
                        principalTable: "Messages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_AppRoles_RoleName",
                schema: "auth",
                table: "AppRoles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_MessageId",
                schema: "chat",
                table: "Attachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_UserId",
                schema: "auth",
                table: "AuthTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogs_CompanyId",
                schema: "auth",
                table: "ContactLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "UQ_ContactLogs_Phone",
                schema: "auth",
                table: "ContactLogs",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_AssignedAgentId",
                schema: "chat",
                table: "Conversations",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_AssignedByUserId",
                schema: "chat",
                table: "Conversations",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClientContactId",
                schema: "chat",
                table: "Conversations",
                column: "ClientContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatedAt",
                schema: "chat",
                table: "Conversations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status",
                schema: "chat",
                table: "Conversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId",
                schema: "chat",
                table: "Conversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_RoleMenus_MenuIndex",
                schema: "auth",
                table: "Menus",
                column: "Index",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RoleMenus_MenuName",
                schema: "auth",
                table: "Menus",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                schema: "chat",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderContactId",
                schema: "chat",
                table: "Messages",
                column: "SenderContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderUserId",
                schema: "chat",
                table: "Messages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_NotificationId",
                schema: "chat",
                table: "NotificationRecipients",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UserId",
                schema: "chat",
                table: "NotificationRecipients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                schema: "chat",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                schema: "chat",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenus_RoleId",
                schema: "auth",
                table: "RoleMenus",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "auth",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                schema: "auth",
                table: "Users",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "AuthTokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "NotificationRecipients",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "RoleMenus",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "Menus",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AppRoles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Conversations",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "auth")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "auth")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.DropTable(
                name: "ContactLogs",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Companies",
                schema: "crm");
        }
    }
}
