using InternFlow.Domain.Internships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class MentorAssignmentConfiguration : IEntityTypeConfiguration<MentorAssignment>
{
    public void Configure(EntityTypeBuilder<MentorAssignment> builder)
    {
        // 1. Tên bảng & Check Constraint
        builder.ToTable("MentorAssignments", "dbo", t =>
        {
            t.HasCheckConstraint("CK_MA_Ended", "[EndedAt] IS NULL OR [EndedAt] >= [AssignedAt]");
        });

        // 2. Khóa chính (BIGINT IDENTITY)
        builder.HasKey(ma => ma.Id)
               .HasName("PK_MentorAssignments");

        builder.Property(ma => ma.Id)
               .UseIdentityColumn();

        builder.Property(ma => ma.InternshipId).HasColumnName("BatchId");

        // 3. Cấu hình các thuộc tính
        builder.Property(ma => ma.AssignedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(ma => ma.EndedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        builder.Property(ma => ma.EndReason)
               .HasMaxLength(200)
               .IsUnicode(true);

        // Cột tính toán IsCurrent trong DB (Computed Column)
        builder.Property(ma => ma.IsCurrent)
               .HasComputedColumnSql("CASE WHEN [EndedAt] IS NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", stored: false);

        // 4. Các Chỉ mục (Indexes)
        // Đảm bảo mỗi Intern chỉ có duy nhất 1 Mentor hiện tại trong 1 đợt thực tập
        builder.HasIndex(ma => new { ma.InternshipId, ma.InternId })
               .IsUnique()
               .HasFilter("[EndedAt] IS NULL")
               .HasDatabaseName("UX_MA_OneCurrentMentor");

        // Kiểm tra phân quyền nhanh: "Mentor này hiện đang hướng dẫn những Intern nào trong đợt?"
        builder.HasIndex(ma => new { ma.MentorId, ma.InternshipId })
               .IncludeProperties(ma => ma.InternId)
               .HasFilter("[EndedAt] IS NULL")
               .HasDatabaseName("IX_MA_CurrentByMentor");

        // Xem toàn bộ lịch sử phân công của Intern trong đợt (kể cả các mentor cũ)
        builder.HasIndex(ma => new { ma.InternshipId, ma.InternId, ma.AssignedAt })
               .IsDescending(false, false, true)
               .HasDatabaseName("IX_MA_HistoryByIntern");

        // 5. Khóa ngoại ghép đa cấp (Composite Foreign Keys)
        // Ràng buộc Intern phải thuộc đợt thực tập này
        builder.HasOne(ma => ma.BatchIntern)
               .WithMany(bi => bi.MentorAssignments)
               .HasForeignKey(ma => new { ma.InternshipId, ma.InternId })
               .HasConstraintName("FK_MA_BatchIntern")
               .OnDelete(DeleteBehavior.Restrict);

        // Ràng buộc Mentor phải thuộc đợt thực tập này
        builder.HasOne(ma => ma.BatchMentor)
               .WithMany(bm => bm.MentorAssignments)
               .HasForeignKey(ma => new { ma.InternshipId, ma.MentorId })
               .HasConstraintName("FK_MA_BatchMentor")
               .OnDelete(DeleteBehavior.Restrict);

        // Tham chiếu Batch (Internship)
        builder.HasOne(ma => ma.Internship)
               .WithMany(b => b.MentorAssignments)
               .HasForeignKey(ma => ma.InternshipId)
               .OnDelete(DeleteBehavior.Restrict);

        // Tham chiếu người phân công
        builder.HasOne(ma => ma.AssignedByUser)
               .WithMany(u => u.AssignedMentorAssignments)
               .HasForeignKey(ma => ma.AssignedBy)
               .HasConstraintName("FK_MA_AssignedBy")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
