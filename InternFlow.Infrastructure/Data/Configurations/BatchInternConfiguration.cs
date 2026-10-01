using InternFlow.Domain.Entities;
using InternFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class BatchInternConfiguration : IEntityTypeConfiguration<BatchIntern>
{
    public void Configure(EntityTypeBuilder<BatchIntern> builder)
    {
        // 1. Tên bảng & Check Constraints
        builder.ToTable("BatchInterns", "dbo", t =>
        {
            t.HasCheckConstraint("CK_BI_Role", "[InternRole] = 'Intern'");
            t.HasCheckConstraint("CK_BI_Status", "[Status] IN ('InProgress', 'Completed', 'DroppedOut', 'Failed')");
        });

        // 2. Khóa chính ghép (Composite Primary Key)
        builder.HasKey(bi => new { bi.BatchId, bi.InternId })
               .HasName("PK_BatchInterns");

        // 3. Cấu hình các thuộc tính
        builder.Property(bi => bi.InternRole)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)
               .HasDefaultValue(UserRole.Intern)
               .IsRequired();

        builder.Property(bi => bi.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)
               .HasDefaultValue(BatchInternStatus.InProgress)
               .IsRequired();

        builder.Property(bi => bi.JoinedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(bi => bi.Notes)
               .HasMaxLength(500)
               .IsUnicode(true);

        // 4. Các Chỉ mục
        builder.HasIndex(bi => bi.InternId)
               .HasDatabaseName("IX_BI_Intern");

        builder.HasIndex(bi => new { bi.BatchId, bi.Status })
               .HasDatabaseName("IX_BI_Status");

        // 5. Khóa ngoại
        // Tham chiếu Batch
        builder.HasOne(bi => bi.Batch)
               .WithMany(b => b.BatchInterns)
               .HasForeignKey(bi => bi.BatchId)
               .HasConstraintName("FK_BI_Batch")
               .OnDelete(DeleteBehavior.Cascade);

        // Khóa ngoại ghép (InternId, InternRole) -> Users(Id, Role)
        builder.HasOne(bi => bi.Intern)
               .WithMany(u => u.BatchInterns)
               .HasPrincipalKey(u => new { u.Id, u.Role })
               .HasForeignKey(bi => new { bi.InternId, bi.InternRole })
               .HasConstraintName("FK_BI_Intern")
               .OnDelete(DeleteBehavior.Restrict);

        // Tham chiếu User thêm vào đợt
        builder.HasOne(bi => bi.AddedByUser)
               .WithMany(u => u.AddedBatchInterns)
               .HasForeignKey(bi => bi.AddedBy)
               .HasConstraintName("FK_BI_AddedBy")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
