using InternFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 1. Tên bảng & Check Constraint
        builder.ToTable("Users", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Admin', 'Mentor', 'Intern')");
        });

        // 2. Khóa chính (Primary Key - UNIQUEIDENTIFIER / Guid)
        builder.HasKey(u => u.Id)
               .HasName("PK_Users");

        builder.Property(u => u.Id)
               .HasDefaultValueSql("NEWID()")
               .ValueGeneratedOnAdd();

        // 3. Khóa thay thế ghép (Alternate Key) để bảng con ép vai trò
        // Sinh ra: CONSTRAINT UQ_Users_Id_Role UNIQUE (Id, Role)
        builder.HasAlternateKey(u => new { u.Id, u.Role })
               .HasName("UQ_Users_Id_Role");

        // 4. Cấu hình chi tiết từng cột
        builder.Property(u => u.Email)
               .HasMaxLength(256)
               .IsUnicode(true)          // NVARCHAR(256)
               .IsRequired();

        // Chỉ số duy nhất cho Email: CONSTRAINT UQ_Users_Email UNIQUE (Email)
        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("UQ_Users_Email");

        builder.Property(u => u.PasswordHash)
               .HasMaxLength(255)
               .IsUnicode(false)         // VARCHAR(255) - Tối ưu lưu trữ ASCII
               .IsRequired();

        builder.Property(u => u.FullName)
               .HasMaxLength(150)
               .IsUnicode(true)          // NVARCHAR(150)
               .IsRequired();

        // Chuyển đổi Enum UserRole <-> VARCHAR(20) trong DB
        builder.Property(u => u.Role)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)         // VARCHAR(20)
               .IsRequired();

        builder.Property(u => u.IsActive)
               .HasDefaultValue(true)
               .HasColumnName("IsActive");

        builder.Property(u => u.FailedLoginCount)
               .HasDefaultValue(0);

        builder.Property(u => u.LockoutEnd)
               .HasColumnType("datetime2")
               .IsRequired(false);

        builder.Property(u => u.LastLoginAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        builder.Property(u => u.CreatedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(u => u.UpdatedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        // 5. Cột RowVersion (Optimistic Concurrency)
        builder.Property(u => u.RowVersion)
               .IsRowVersion();          // Tự động nhận diện kiểu ROWVERSION trong SQL Server
    }
}
