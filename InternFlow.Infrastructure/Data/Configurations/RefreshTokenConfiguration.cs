using InternFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // 1. Tên bảng
        builder.ToTable("RefreshTokens", "dbo");

        // 2. Khóa chính (BIGINT IDENTITY)
        builder.HasKey(rt => rt.Id)
               .HasName("PK_RefreshTokens");

        builder.Property(rt => rt.Id)
               .UseIdentityColumn();

        // 3. Cột TokenHash (CHAR(64)) & Unique Index
        builder.Property(rt => rt.TokenHash)
               .HasMaxLength(64)
               .IsFixedLength()
               .IsUnicode(false)
               .IsRequired();

        builder.HasIndex(rt => rt.TokenHash)
               .IsUnique()
               .HasDatabaseName("UQ_RefreshTokens_Hash");

        // 4. Các thuộc tính
        builder.Property(rt => rt.FamilyId)
               .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
               .HasColumnType("datetime2")
               .IsRequired();

        builder.Property(rt => rt.CreatedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(rt => rt.CreatedByIp)
               .HasMaxLength(45)
               .IsUnicode(false);

        builder.Property(rt => rt.UserAgent)
               .HasMaxLength(500)
               .IsUnicode(true);

        builder.Property(rt => rt.UsedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        builder.Property(rt => rt.RevokedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        builder.Property(rt => rt.RevokedReason)
               .HasMaxLength(100)
               .IsUnicode(true);

        // 5. Các Chỉ mục (Indexes)
        // Tra cứu danh sách token của user khi logout hoặc kiểm tra phiên
        builder.HasIndex(rt => rt.UserId)
               .IncludeProperties(rt => new { rt.FamilyId, rt.ExpiresAt, rt.RevokedAt })
               .HasDatabaseName("IX_RefreshTokens_UserId");

        // Tra cứu thu hồi toàn bộ token trong Family khi phát hiện Refresh Token bị dùng lại
        builder.HasIndex(rt => rt.FamilyId)
               .HasDatabaseName("IX_RefreshTokens_Family");

        // Hỗ trợ Background Service / Hangfire định kỳ quét dọn dẹp các token đã hết hạn
        builder.HasIndex(rt => rt.ExpiresAt)
               .IncludeProperties(rt => rt.RevokedAt)
               .HasDatabaseName("IX_RefreshTokens_Purge");

        // 6. Khóa ngoại (Relationships)
        builder.HasOne(rt => rt.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(rt => rt.UserId)
               .HasConstraintName("FK_RT_Users")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.ReplacedByToken)
               .WithMany()
               .HasForeignKey(rt => rt.ReplacedByTokenId)
               .HasConstraintName("FK_RT_ReplacedBy")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
