using CustomerService.API.Models;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace CustomerService.API.Data.Context;

public partial class CustomerSupportContext : DbContext
{
    private readonly IEnumerable<SaveChangesInterceptor> _interceptors;
    public CustomerSupportContext(DbContextOptions<CustomerSupportContext> options,
        IEnumerable<SaveChangesInterceptor> interceptors)
        : base(options)
    {
        _interceptors = interceptors;
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuthToken> AuthTokens { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<RoleMenu> MenuRoles { get; set; }
    public virtual DbSet<ContactLog> ContactLogs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }
    public virtual DbSet<SystemParam> SystemParams { get; set; }
    public virtual DbSet<ConversationHistoryLog> ConversationHistoryLogs { get; set; }
    public virtual DbSet<OpeningHour> OpeningHours { get; set; }

    public DbSet<MessageStatusHistory> MessageStatusHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (_interceptors != null)
        {
            foreach (var interceptor in _interceptors)
            {
                optionsBuilder.AddInterceptors(interceptor);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.ToTable("AppRoles", "auth");

            entity.HasIndex(e => e.RoleName, "UQ_AppRoles_RoleName").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Menu>(entity => {
            entity.HasKey(m=> m.MenuId);

            entity.ToTable("Menus", "auth");

            entity.HasIndex(e => e.Name, "UQ_RoleMenus_MenuName").IsUnique();
            entity.HasIndex(e => e.Index, "UQ_RoleMenus_MenuIndex").IsUnique();

            entity.Property(e=> e.Description).HasMaxLength(255);
            entity.Property(e=> e.Icon).HasMaxLength(255);
            entity.Property(e=> e.Url).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        });

        // ContactLog configuration
        modelBuilder.Entity<ContactLog>(entity =>
        {
            entity.ToTable("ContactLogs", "auth");

            entity.HasKey(e => e.Id)
                .HasName("PK_ContactLogs");

            entity.Property(e => e.IdType)
                  .HasConversion<string>()
                  .HasMaxLength(30);

            entity.HasIndex(e => e.Phone)
                .IsUnique()
                .HasDatabaseName("UQ_ContactLogs_Phone");

            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(e => e.IdCard)
                .HasMaxLength(30)
                .IsRequired(false);

            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.WaId)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(e => e.WaName)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(e => e.WaUserId)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ContactStatus.New);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.IsVerified)
            .IsRequired(false);

            entity.Property(e => e.VerifiedId)
            .IsRequired(false);

            entity.Property(e => e.VerifiedAt)
               .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(cl => cl.VerifiedBy)
              .WithMany(u => u.ContactLogs)
              .HasForeignKey(cl => cl.VerifiedId)
              .OnDelete(DeleteBehavior.Restrict)
              .HasConstraintName("FK_ContactLogs_verifyUser");

            entity.Property(e => e.IsProvidingData)
            .IsRequired(false);

            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(e => e.Company)
                .WithMany(c => c.ContactLogs)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ContactLogs_Companies");

            entity.HasMany(e => e.ConversationClient)
                .WithOne(c => c.ClientContact)
                .HasForeignKey(c => c.ClientContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK_Attachments");

            entity.ToTable("Attachments", "chat");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FileName).HasMaxLength(200);
            entity.Property(e => e.MediaId).HasMaxLength(100);
            entity.Property(e => e.MediaUrl).HasMaxLength(500);

            entity.HasOne(d => d.Message).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.MessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Attachments_Messages");

        });

        modelBuilder.Entity<AuthToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);

            entity.ToTable("AuthTokens", "auth");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.TokenType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AuthTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuthTokens_Users");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies", "crm");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations", "chat");
            entity.HasKey(e => e.ConversationId);

            entity.Property(e => e.Priority)
                  .HasConversion<int>()
                  .HasDefaultValue(PriorityLevel.Normal);

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(ConversationStatus.New);

            entity.Property(e => e.AssignmentState)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(AssignmentState.Unassigned);

            entity.Property(e => e.Justification)
                    .HasMaxLength(500)
                    .IsRequired(false);

            entity.Property(e => e.AssignmentComment)
            .HasMaxLength(500)
            .IsRequired(false);

            entity.Property(e => e.AssignmentComment)
            .HasMaxLength(30)
            .IsRequired(false);

            entity.Property(e => e.Initialized)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime2")
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.FirstResponseAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);

            entity.Property(e => e.AssignedAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);
            entity.Property(e => e.AgentRequestAt)
                .HasColumnType("datetime2")
                      .IsRequired(false);

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);

            // <<-- Ya tenías ClosedAt configurado: >>
            entity.Property(e => e.ClosedAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);

            entity.Property(e => e.IncompletedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

            // <<-- AGREGAR EL MAPEADO DE WarningSentAt justo aquí: >> 
            entity.Property(e => e.WarningSentAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);

            entity.Property(e => e.AssignmentResponseAt)
            .HasColumnType("datetime2")
            .IsRequired(false);

            entity.Property(e => e.IsArchived)
                  .HasDefaultValue(false);

            entity.Property(e => e.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();

            entity.Property(e => e.Tags)
                  .HasColumnType("nvarchar(max)");

            // Relaciones con FK:
            entity.HasOne(e => e.ClientContact)
                  .WithMany(cl => cl.ConversationClient)
                  .HasForeignKey(e => e.ClientContactId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Conversations_ClientContact");

            entity.HasOne(e => e.AssignedAgent)
                  .WithMany(u => u.ConversationAssignedAgentNavigations)
                  .HasForeignKey(e => e.AssignedAgentId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Conversations_AssignedAgent");

            entity.HasOne(e => e.AssignedByUser)
                  .WithMany(u => u.ConversationAssignedByNavigations)
                  .HasForeignKey(e => e.AssignedByUserId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Conversations_AssignedByUser");

            entity.Property(e => e.AgentLastReadMessageId)
              .HasColumnName("AgentLastReadMessageId")
              .HasColumnType("int")
              .IsRequired(false);


            entity.Property(e => e.AssignerLastReadMessageId)
             .HasColumnName("AssignerLastReadMessageId")
             .HasColumnType("int")
             .IsRequired(false);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages", "chat");
            entity.HasKey(e => e.MessageId);

            entity.Property(e => e.MessageType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(MessageType.Text);

            entity.Property(e => e.ExternalId)
            .HasMaxLength(255)
            .IsRequired(false);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(MessageStatus.Sent);

            entity.Property(e => e.SentAt)
                .HasColumnType("datetimeoffset(7)")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.DeliveredAt)
                .HasColumnType("datetimeoffset(7)")
                .IsRequired(false);

            entity.Property(e => e.ReadAt)
                .HasColumnType("datetimeoffset(7)")
                .IsRequired(false);

            entity.Property(e => e.InteractiveId)
            .IsRequired(false);

            entity.Property(e => e.InteractiveTitle)
            .HasMaxLength(255)
            .IsRequired(false);

            entity.Property(e => e.ExternalId)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Messages_Conversations");

            entity.HasOne(e => e.SenderUser)
                .WithMany(u => u.Messages)
                .HasForeignKey(e => e.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Messages_SenderUser");

            entity.HasOne(e => e.SenderContact)
                .WithMany(cl => cl.MessagesSent)
                .HasForeignKey(e => e.SenderContactId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Messages_SenderContact");

            entity.HasCheckConstraint(
                "CK_Message_OneSender",
                "(SenderUserId IS NOT NULL AND SenderContactId IS NULL) OR (SenderUserId IS NULL AND SenderContactId IS NOT NULL)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "auth", tb =>
            {
                tb.IsTemporal(ttb =>
                {
                    ttb.UseHistoryTable("UsersHistory", "auth");
                    ttb.HasPeriodStart("ValidFrom").HasColumnName("ValidFrom");
                    ttb.HasPeriodEnd("ValidTo").HasColumnName("ValidTo");
                });
            });

            entity.Property(e => e.ConcurrencyStamp)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FullName)
                .HasMaxLength(100);

            entity.Property(e => e.Identifier)
                .HasMaxLength(50);

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256);

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.SecurityStamp)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.Property(e => e.LastOnline);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Users_Companies");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.ToTable("UserRoles", "auth");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(e => new { e.MenuId, e.RoleId });
            entity.ToTable("RoleMenus", "auth");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleMenus)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RoleMenus_Roles");

            entity.HasOne(d => d.Menu).WithMany(p => p.RoleMenus)
            .HasForeignKey(p => p.MenuId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RoleMenus_Menus");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies", "crm");
            entity.HasKey(c => c.CompanyId);
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(c => c.UpdatedAt).IsRequired(false);
            //entity.Property(c => c.RowVersion)
            //    .IsRowVersion()
            //    .IsConcurrencyToken();
            entity.Property(c => c.Description).HasMaxLength(255);
            entity.Property(c => c.Address).HasMaxLength(255);

        });

        modelBuilder.Entity<Notification>(entity => {
            entity.HasKey(n => n.Id);

            entity.ToTable("Notifications", "chat");

            entity.Property(n => n.Id)
                  .ValueGeneratedOnAdd();

            entity.Property(n => n.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

            entity.Property(n => n.Payload).IsRequired();

            entity.Property(n => n.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<NotificationRecipient>(entity => {
            entity.ToTable("NotificationRecipients", "chat");
            entity.HasKey(nr => nr.NotificationRecipientId);
            entity.HasOne(nr => nr.Notification)
                  .WithMany(n => n.Recipients)
                  .HasForeignKey(nr => nr.NotificationId);

            entity.HasOne(nr => nr.User)
                  .WithMany(u => u.NotificationRecipients)
                  .HasForeignKey(nr => nr.UserId);

            entity.Property(nr => nr.IsRead).HasDefaultValue(false);
        });

        modelBuilder.Entity<SystemParam>(entity => {
            entity.HasKey(sp => sp.Id);

            entity.ToTable("SystemParams", "auth");

            entity.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(sp => sp.Description)
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(sp => sp.Value)
              .IsRequired()
              .HasColumnType("nvarchar(max)");

            entity.Property(sp => sp.Type)
              .HasConversion<string>()
              .HasMaxLength(50)
              .HasColumnType("nvarchar(50)")
              .IsRequired();


            entity.Property(sp => sp.CreatedAt)
       .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(sp => sp.UpdatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(sp => sp.CreateBy)
                .IsRequired(false);
            entity.Property(sp => sp.UpdateBy)
                .IsRequired(false);
            entity.Property(sp=> sp.IsActive)
                            .HasDefaultValue(false);
            entity.Property(sp => sp.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ConversationHistoryLog>(entity =>
        {
            entity.ToTable("ConversationHistoryLog", "chat");

            entity.HasKey(chl => chl.Id);

            entity.HasOne(chl => chl.Conversation)
                  .WithMany(c => c.ConversationHistoryLogs)
                  .HasForeignKey(chl => chl.ConversationId);

            entity.Property(chl => chl.ChangedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(chl => chl.SourceIp)
                  .HasMaxLength(45);

            entity.Property(chl => chl.UserAgent)
                  .HasMaxLength(200);

            entity.HasOne(e => e.Conversation)
              .WithMany(u => u.ConversationHistoryLogs)
              .HasForeignKey(e => e.ConversationId)
              .OnDelete(DeleteBehavior.Restrict)
              .HasConstraintName("FK_ConversationHistoryLog_Conversation");

            entity.HasOne(e => e.ChangedByUser)
              .WithMany(u => u.ConversationHistoryLogs)
              .HasForeignKey(e => e.ChangedByUserId)
              .OnDelete(DeleteBehavior.Restrict)
              .HasConstraintName("FK_ConversationHistoryLog_ChangedByUserId");
        });

        modelBuilder.Entity<OpeningHour>(entity =>
        {
            entity.ToTable("OpeningHour", "crm");
            entity.HasKey(oh => oh.Id);

            entity.Property(oh => oh.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(oh => oh.Description)
                .HasMaxLength(255)
                .IsRequired(false);

            // RecurrenceType como string
            entity.Property(oh => oh.Recurrence)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // DaysOfWeek[] <-> "Mon,Tue,Wed"
            var dowConverter = new ValueConverter<DayOfWeek[], string>(
                v => string.Join(',', v.Select(d => d.ToString())),
                s => s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(str => Enum.Parse<DayOfWeek>(str))
                      .ToArray());

            entity.Property(oh => oh.DaysOfWeek)
                .HasConversion(dowConverter)
                .HasMaxLength(50)
                .IsRequired(false);

            // HolidayDate (DayMonth) <-> "dd/MM"
            var dayMonthConverter = new ValueConverter<DayMonth, string>(
                dm => dm.ToString(),
                s => DayMonth.Parse(s));

            entity.Property(oh => oh.HolidayDate)
                .HasConversion(dayMonthConverter)
                .HasMaxLength(5)
                .IsRequired(false);

            // SpecificDate como DATE
            entity.Property(oh => oh.SpecificDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(oh => oh.IsWorkShift)
            .IsRequired(false);
            entity.Property(oh=> oh.IsHolidayMoved)
            .IsRequired (false);
            entity.Property(oh => oh.HolidayMovedFrom)
            .HasColumnType("date")
            .IsRequired(false);
            entity.Property(oh=> oh.HolidayMoveTo)
            .HasColumnType ("date")
            .IsRequired(false);

            // Horas como TIME
            entity.Property(oh => oh.StartTime)
                .HasColumnType("time")
                .IsRequired(false);
            entity.Property(oh => oh.EndTime)
                .HasColumnType("time")
                .IsRequired(false);

            // Vigencia como DATE
            entity.Property(oh => oh.EffectiveFrom)
                .HasColumnType("date")
                .IsRequired(false);
            entity.Property(oh => oh.EffectiveTo)
                .HasColumnType("date")
                .IsRequired(false);

            // Estado por defecto
            entity.Property(oh => oh.IsActive)
                .HasDefaultValue(true);

            entity.Property(oh => oh.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(oh => oh.UpdatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.OpeningHoursCreatedBy)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OpeningHour_CreatedBy");

            entity.HasOne(e => e.UpdatedBy)
                .WithMany(u => u.OpeningHoursUpdatedBy)
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OpeningHour_UpdatedBy");
        });

        modelBuilder.Entity<WorkShift_User>(entity =>
        {
            entity.ToTable("WorkShift_User", "crm");
            entity.HasKey(ws => ws.Id);

            entity.Property(ws => ws.IsActive)
                .HasDefaultValue(true);
            entity.Property(ws => ws.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(ws => ws.UpdatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(ws => ws.ValidFrom)
                .HasColumnType("date")
                .IsRequired(false);
            entity.Property(ws => ws.ValidTo)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(ws => ws.RowVersion)
                .IsRowVersion();

            // Relaciones
            entity.HasOne(ws => ws.AssignedUser)
                .WithMany(u => u.WorkShift_UsersAssignedTo)
                .HasForeignKey(ws => ws.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_WorkShift_User_AssignedUser");

            entity.HasOne(ws => ws.CreatedBy)
                .WithMany(u => u.WorkShift_UsersCreatedBy)
                .HasForeignKey(ws => ws.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_WorkShift_User_CreatedBy");

            entity.HasOne(ws => ws.UpdatedBy)
                .WithMany(u => u.WorkShift_UsersUpdatedBy)
                .HasForeignKey(ws => ws.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_WorkShift_User_UpdatedBy");

            entity.HasOne(ws => ws.OpeningHour)
                .WithMany(oh => oh.WorkShift_Users)
                .HasForeignKey(ws => ws.OpeningHourId)
                .HasConstraintName("FK_OpeningHour_WorkShift_User");
        });
        modelBuilder.Entity<MessageStatusHistory>(entity =>
        {
            entity.ToTable("MessageStatusHistory");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MessageId)
                  .IsRequired();

            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();

            entity.Property(e => e.Timestamp)
                  .IsRequired();

            entity.Property(e => e.Metadata)
                  .HasColumnType("nvarchar(max)");

            // Índice compuesto para búsquedas por mensaje, estado y UpdatedAt
            entity.HasIndex(e => new { e.MessageId, e.Status });

            entity.HasOne(e => e.Message)
                  .WithMany(m => m.StatusHistories)
                  .HasForeignKey(e => e.MessageId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_MessageStatusHistory_Message");
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}