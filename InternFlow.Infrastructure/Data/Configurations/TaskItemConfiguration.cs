using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks", "dbo");
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
        builder.HasIndex(t => new { t.ProjectId, t.Status });
        builder.HasIndex(t => t.AssigneeId);
        builder.HasOne(t => t.Project).WithMany(p => p.Tasks).HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Assignee).WithMany(u => u.AssignedTasks).HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.SetNull);
    }
}
