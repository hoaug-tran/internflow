using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
{
    public void Configure(EntityTypeBuilder<WorkLog> builder)
    {
        builder.ToTable("WorkLogs", "dbo", t =>
        {
            t.HasCheckConstraint("CK_WorkLogs_Hours",
                "[Hours] > 0 AND [Hours] <= 24");
            t.HasCheckConstraint("CK_WorkLogs_Description",
                "LEN(LTRIM(RTRIM([Description]))) > 0");
        });

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(w => w.WorkDate).HasColumnType("date");
        builder.Property(w => w.Hours).HasPrecision(4, 2);
        builder.Property(w => w.Description).HasMaxLength(2000).IsRequired();
        builder.Property(w => w.DeletedAt).HasColumnType("datetime2");
        builder.Property(w => w.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(w => w.UpdatedAt).HasColumnType("datetime2");
        builder.Property(w => w.RowVersion).IsRowVersion();

        builder.HasIndex(w => new { w.TaskItemId, w.UserId, w.WorkDate })
               .IsUnique()
               .HasFilter("[DeletedAt] IS NULL")
               .HasDatabaseName("UX_WorkLogs_Task_User_Date_Active");

        builder.HasIndex(w => new { w.UserId, w.WorkDate })
               .IncludeProperties(w => new { w.TaskItemId, w.Hours })
               .HasFilter("[DeletedAt] IS NULL")
               .HasDatabaseName("IX_WorkLogs_UserId_WorkDate");

        builder.HasIndex(w => w.TaskItemId)
               .HasDatabaseName("IX_WorkLogs_TaskItemId");

        builder.HasOne(w => w.TaskItem).WithMany(t => t.WorkLogs)
               .HasForeignKey(w => w.TaskItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.User).WithMany(u => u.WorkLogs)
               .HasForeignKey(w => w.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
