using System;
using System.Collections.Generic;
using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Data.Context;

public partial class CustomerSupportContext : DbContext
{
    public CustomerSupportContext(DbContextOptions<CustomerSupportContext> options)
        : base(options)
    {
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
    public SystemParam SystemParams { get; set; }
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
                .HasMaxLength(20)
                .HasDefaultValue(ContactStatus.New);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.UpdatedAt)
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

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);

            // <<-- Ya tenías ClosedAt configurado: >>
            entity.Property(e => e.ClosedAt)
                  .HasColumnType("datetime2")
                  .IsRequired(false);

            // <<-- AGREGAR EL MAPEADO DE WarningSentAt justo aquí: >> 
            entity.Property(e => e.WarningSentAt)
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
            entity.HasKey(n => n.NotificationId);

            entity.ToTable("Notifications", "chat");

            entity.Property(n => n.NotificationId)
                  .ValueGeneratedOnAdd();

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
            entity.ToTable("SystemParams", "auth");
            entity.HasKey(sp => sp.Id);
            entity.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(sp => sp.Description)
                .HasMaxLength(255)
                .IsRequired(false);
            entity.Property(sp => sp.CreateAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(sp => sp.UpdateAt)
                .IsRequired(false);
            entity.Property(sp => sp.CreateBy)
                .IsRequired();
            entity.Property(sp => sp.UpdateBy)
                .IsRequired();
            entity.Property(sp => sp.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}