using InternFlow.Domain.Evaluations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class EvaluationDetailConfiguration : IEntityTypeConfiguration<EvaluationDetail>
{
    public void Configure(EntityTypeBuilder<EvaluationDetail> builder)
    {
        builder.ToTable("EvaluationDetails", "dbo", t => t.HasCheckConstraint("CK_EvaluationDetails_Score", "[Score] >= 0 AND [Score] <= 10"));
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(d => d.Score).HasPrecision(4, 2);
        builder.Property(d => d.Comment).HasMaxLength(2000);
        builder.HasIndex(d => new { d.EvaluationId, d.CriterionId }).IsUnique();
        builder.HasOne(d => d.Evaluation).WithMany(e => e.Details).HasForeignKey(d => d.EvaluationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Criterion).WithMany(c => c.EvaluationDetails).HasForeignKey(d => d.CriterionId).OnDelete(DeleteBehavior.Restrict);
    }
}
