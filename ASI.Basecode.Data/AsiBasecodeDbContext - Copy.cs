/*using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDbContext : DbContext
    {
        public AsiBasecodeDbContext()
        {
        }

        public AsiBasecodeDbContext(DbContextOptions<AsiBasecodeDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<ArticleCategory> ArticleCategories { get; set; }
        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<CategoryType> CategoryTypes { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotificationType> NotificationTypes { get; set; }
        public virtual DbSet<PerformanceReport> PerformanceReports { get; set; }
        public virtual DbSet<PriorityType> PriorityTypes { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StatusType> StatusTypes { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TeamMember> TeamMembers { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketAssignment> TicketAssignments { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Addr=localhost;database=AsiBasecodeDb;Integrated Security=False;Trusted_Connection=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.ActivityId);

                entity.ToTable("ActivityLog");

                entity.Property(e => e.ActivityId)
                    .HasMaxLength(256)
                    .HasColumnName("activity_ID");

                entity.Property(e => e.ActivityDate)
                    .HasColumnType("datetime")
                    .HasColumnName("activityDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("activityType");

                entity.Property(e => e.Details).HasColumnName("details");

                entity.Property(e => e.TicketId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.ActivityLogs)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActivityLog_Ticket");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ActivityLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActivityLog_User");
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admin");
                entity.Property(e => e.AdminId)
                    .HasMaxLength(256)
                    .HasColumnName("admin_ID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.IsSuper).HasColumnName("isSuper");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("password");
            });

            modelBuilder.Entity<ArticleCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.ToTable("ArticleCategory");

                entity.Property(e => e.CategoryId)
                    .HasMaxLength(256)
                    .HasColumnName("category_ID");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("categoryName");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");
            });

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("Attachment");

                entity.Property(e => e.AttachmentId)
                    .HasMaxLength(256)
                    .HasColumnName("attachment_ID");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnName("content");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("name");

                entity.Property(e => e.TicketId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("type");

                entity.Property(e => e.UploadedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("uploadedDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attachment_Ticket");
            });

            modelBuilder.Entity<CategoryType>(entity =>
            {
                entity.ToTable("CategoryType");

                entity.Property(e => e.CategoryTypeId)
                    .HasMaxLength(256)
                    .HasColumnName("categoryType_ID");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("categoryName");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");

                entity.Property(e => e.FeedbackId)
                    .HasMaxLength(256)
                    .HasColumnName("feedback_ID");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createdDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FeedbackContent)
                    .IsRequired()
                    .HasColumnName("feedbackContent");
                entity.Property(e => e.TicketId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Feedback_Ticket");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Feedback_User");
            });

            modelBuilder.Entity<KnowledgeBaseArticle>(entity =>
            {
                entity.HasKey(e => e.ArticleId);

                entity.ToTable("KnowledgeBaseArticle");

                entity.Property(e => e.ArticleId)
                    .HasMaxLength(256)
                    .HasColumnName("article_ID");

                entity.Property(e => e.AuthorId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("author_ID");
                entity.Property(e => e.CategoryId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("category_ID");
                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnName("content");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createdDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("title");

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedDate");
                entity.HasOne(d => d.Author)
                    .WithMany(p => p.KnowledgeBaseArticles)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KnowledgeBaseArticle_User");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.KnowledgeBaseArticles)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KnowledgeBaseArticle_ArticleCategory");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.NotificationId)
                    .HasMaxLength(256)
                    .HasColumnName("notification_ID");

                entity.Property(e => e.NotificationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("notificationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.NotificationTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("notificationType_ID");

                entity.Property(e => e.TicketId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.HasOne(d => d.NotificationType)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.NotificationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notification_NotificationType");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notification_Ticket");
            });

            modelBuilder.Entity<NotificationType>(entity =>
            {
                entity.ToTable("NotificationType");

                entity.Property(e => e.NotificationTypeId)
                    .HasMaxLength(256)
                    .HasColumnName("notificationType_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<PerformanceReport>(entity =>
            {
                entity.HasKey(e => e.ReportId);

                entity.ToTable("PerformanceReport");

                entity.Property(e => e.ReportId)
                    .HasMaxLength(256)
                    .HasColumnName("report_ID");

                entity.Property(e => e.AssignedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("assignedDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.AverageResolutionTime).HasColumnName("averageResolutionTime");

                entity.Property(e => e.ResolvedTickets).HasColumnName("resolvedTickets");
            });

            modelBuilder.Entity<PriorityType>(entity =>
            {
                entity.ToTable("PriorityType");

                entity.Property(e => e.PriorityTypeId)
                    .HasMaxLength(256)
                    .HasColumnName("priorityType_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.PriorityName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("priorityName");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.RoleId)
                    .HasMaxLength(256)
                    .HasColumnName("role_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("roleName");
            });

            modelBuilder.Entity<StatusType>(entity =>
            {
                entity.ToTable("StatusType");

                entity.Property(e => e.StatusTypeId)
                    .HasMaxLength(256)
                    .HasColumnName("statusType_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.StatusName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("statusName");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");

                entity.Property(e => e.TeamId)
                    .HasMaxLength(256)
                    .HasColumnName("team_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasKey(e => new { e.TeamId, e.UserId });

                entity.ToTable("TeamMember");

                entity.Property(e => e.TeamId)
                    .HasMaxLength(256)
                    .HasColumnName("team_ID");

                entity.Property(e => e.UserId)
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.Property(e => e.ReportId)
                    .HasMaxLength(256)
                    .HasColumnName("report_ID");

                entity.HasOne(d => d.Report)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.ReportId)
                    .HasConstraintName("FK_TeamMember_PerformanceReport");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeamMember_Team");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeamMember_User");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket");

                entity.Property(e => e.TicketId)
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.Property(e => e.CategoryTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("categoryType_ID");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createdDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IssueDescription)
                    .IsRequired()
                    .HasMaxLength(800)
                    .HasColumnName("issueDescription");

                entity.Property(e => e.PriorityTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("priorityType_ID");

                entity.Property(e => e.ResolvedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("resolvedDate");

                entity.Property(e => e.StatusTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("statusType_ID");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("subject");

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedDate");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.HasOne(d => d.CategoryType)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.CategoryTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_CategoryType");

                entity.HasOne(d => d.PriorityType)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.PriorityTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_PriorityType");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_StatusType");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_User");
            });

            modelBuilder.Entity<TicketAssignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentId);

                entity.ToTable("TicketAssignment");

                entity.Property(e => e.AssignmentId)
                    .HasMaxLength(256)
                    .HasColumnName("assignment_ID");

                entity.Property(e => e.AdminId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("admin_ID");

                entity.Property(e => e.AssignedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("assignedDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TeamId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("team_ID");

                entity.Property(e => e.TicketId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.TicketAssignments)
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketAssignment_Admin");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TicketAssignments)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketAssignment_Team");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.TicketAssignments)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketAssignment_Ticket");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.UserId)
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("createdBy");

                entity.Property(e => e.CreatedTime)
                    .HasColumnType("datetime")
                    .HasColumnName("createdTime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("password");

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("role_ID");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(256)
                    .HasColumnName("updatedBy");

                entity.Property(e => e.UpdatedTime)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedTime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.UserCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Admin_CreatedBy");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");

                entity.HasOne(d => d.UpdatedByNavigation)
                    .WithMany(p => p.UserUpdatedByNavigations)
                    .HasForeignKey(d => d.UpdatedBy)
                    .HasConstraintName("FK_User_Admin_UpdatedBy");
            });

            modelBuilder.Entity<PerformanceReport>(entity =>
            {
                entity.HasKey(e => e.ReportId);

                entity.ToTable("PerformanceReport");

                entity.Property(e => e.ReportId)
                    .HasMaxLength(256)
                    .HasColumnName("report_ID");

                entity.Property(e => e.AssignedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("assignedDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.AverageResolutionTime).HasColumnName("averageResolutionTime");

                entity.Property(e => e.ResolvedTickets).HasColumnName("resolvedTickets");
            });

            modelBuilder.Entity<PriorityType>(entity =>
            {
                entity.ToTable("PriorityType");

                entity.Property(e => e.PriorityTypeId)
                    .HasMaxLength(256)
                    .HasColumnName("priorityType_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.PriorityName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("priorityName");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.RoleId)
                    .HasMaxLength(256)
                    .HasColumnName("role_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("roleName");
            });

            modelBuilder.Entity<StatusType>(entity =>
            {
                entity.ToTable("StatusType");

                entity.Property(e => e.StatusTypeId)
                    .HasMaxLength(256)
                    .HasColumnName("statusType_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.StatusName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("statusName");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");

                entity.Property(e => e.TeamId)
                    .HasMaxLength(256)
                    .HasColumnName("team_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasKey(e => new { e.TeamId, e.UserId });

                entity.ToTable("TeamMember");

                entity.Property(e => e.TeamId)
                    .HasMaxLength(256)
                    .HasColumnName("team_ID");

                entity.Property(e => e.UserId)
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.Property(e => e.ReportId)
                    .HasMaxLength(256)
                    .HasColumnName("report_ID");

                entity.HasOne(d => d.Report)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.ReportId)
                    .HasConstraintName("FK_TeamMember_PerformanceReport");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeamMember_Team");

       *//*         entity.HasOne(d => d.User)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeamMember_User");*//*
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket");

                entity.Property(e => e.TicketId)
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.Property(e => e.Attachment).HasColumnName("attachment");

                entity.Property(e => e.CategoryTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("categoryType_ID");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createdDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IssueDescription)
                    .IsRequired()
                    .HasMaxLength(800)
                    .HasColumnName("issueDescription");

                entity.Property(e => e.PriorityTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("priorityType_ID");

                entity.Property(e => e.ResolvedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("resolvedDate");

                entity.Property(e => e.StatusTypeId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("statusType_ID");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("subject");

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedDate");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.HasOne(d => d.CategoryType)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.CategoryTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_CategoryType");

                entity.HasOne(d => d.PriorityType)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.PriorityTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_PriorityType");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_StatusType");

   *//*             entity.HasOne(d => d.User)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ticket_User");*//*
            });

            modelBuilder.Entity<TicketAssignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentId);

                entity.ToTable("TicketAssignment");

                entity.Property(e => e.AssignmentId)
                    .HasMaxLength(256)
                    .HasColumnName("assignment_ID");

                entity.Property(e => e.AdminId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("admin_ID");

                entity.Property(e => e.AssignedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("assignedDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TeamId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("team_ID");

                entity.Property(e => e.TicketId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("ticket_ID");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.TicketAssignments)
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketAssignment_Admin");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TicketAssignments)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketAssignment_Team");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.TicketAssignments)
                    .HasForeignKey(d => d.TicketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketAssignment_Ticket");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.UserId)
                    .HasMaxLength(256)
                    .HasColumnName("user_ID");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("createdBy");

                entity.Property(e => e.CreatedTime)
                    .HasColumnType("datetime")
                    .HasColumnName("createdTime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("password");

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("role_ID");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(256)
                    .HasColumnName("updatedBy");

                entity.Property(e => e.UpdatedTime)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedTime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.UserCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Admin_CreatedBy");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");

                entity.HasOne(d => d.UpdatedByNavigation)
                    .WithMany(p => p.UserUpdatedByNavigations)
                    .HasForeignKey(d => d.UpdatedBy)
                    .HasConstraintName("FK_User_Admin_UpdatedBy");
            });

            

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
*/