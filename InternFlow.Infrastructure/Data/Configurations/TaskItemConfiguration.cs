using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Tasks_Status",
                "[Status] IN ('Todo','InProgress','InReview','Done','Cancelled')");
            t.HasCheckConstraint("CK_Tasks_Priority",
                "[Priority] IN ('Low','Medium','High','Critical')");
            t.HasCheckConstraint("CK_Tasks_Title",
                "LEN(LTRIM(RTRIM([Title]))) > 0");
            t.HasCheckConstraint("CK_Tasks_CompletedAt",
                "([Status] = 'Done' AND [CompletedAt] IS NOT NULL) OR ([Status] <> 'Done' AND [CompletedAt] IS NULL)");
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(t => t.Title).HasMaxLength(250).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(t => t.DueDate).HasColumnType("date");
        builder.Property(t => t.CompletedAt).HasColumnType("datetime2");
        builder.Property(t => t.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(t => t.UpdatedAt).HasColumnType("datetime2");
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasIndex(t => new { t.ProjectId, t.Status })
               .IncludeProperties(t => new { t.AssigneeId, t.Priority, t.DueDate });
        builder.HasIndex(t => new { t.AssigneeId, t.Status })
               .IncludeProperties(t => new { t.ProjectId, t.Priority, t.DueDate });
        builder.HasIndex(t => new { t.ProjectId, t.CreatedAt });
        builder.HasIndex(t => t.CreatedBy);

        builder.HasOne(t => t.Project).WithMany(p => p.Tasks)
               .HasForeignKey(t => t.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Assignee).WithMany(u => u.AssignedTasks)
               .HasForeignKey(t => t.AssigneeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Creator).WithMany()
               .HasForeignKey(t => t.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}