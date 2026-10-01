using InternFlow.Domain.Evaluations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class EvaluationPeriodConfiguration : IEntityTypeConfiguration<EvaluationPeriod>
{
    public void Configure(EntityTypeBuilder<EvaluationPeriod> builder)
    {
        builder.ToTable("EvaluationPeriods", "dbo", t => t.HasCheckConstraint("CK_EvaluationPeriods_Dates", "[EndDate] >= [StartDate]"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.StartDate).HasColumnType("date");
        builder.Property(p => p.EndDate).HasColumnType("date");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(p => p.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(p => p.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(p => new { p.InternshipId, p.Name }).IsUnique();
        builder.HasOne(p => p.Internship).WithMany(i => i.EvaluationPeriods).HasForeignKey(p => p.InternshipId).OnDelete(DeleteBehavior.Cascade);
    }
}
