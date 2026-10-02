using InternFlow.Domain.Evaluations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class CriterionConfiguration : IEntityTypeConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        builder.ToTable("Criteria", "dbo", t => t.HasCheckConstraint("CK_Criteria_Weight", "[Weight] > 0 AND [Weight] <= 100"));
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.Weight).HasPrecision(5, 2);
        builder.Property(c => c.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(c => c.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(c => new { c.EvaluationPeriodId, c.DisplayOrder }).IsUnique();
        builder.HasOne(c => c.EvaluationPeriod).WithMany(p => p.Criteria).HasForeignKey(c => c.EvaluationPeriodId).OnDelete(DeleteBehavior.Cascade);
    }
}
