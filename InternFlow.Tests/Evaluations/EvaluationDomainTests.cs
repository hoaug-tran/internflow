using FluentAssertions;
using InternFlow.Domain.Evaluations;
using Xunit;

namespace InternFlow.Domain.Tests.Evaluations;

public class EvaluationDomainTests
{
    private static List<Criterion> Criteria() =>
        new (string Name, decimal Weight)[]
        {
            ("Thái độ", 20), ("Kỹ năng", 30), ("Nhóm", 20), ("Tiến độ", 20), ("Chủ động", 10)
        }.Select((c, i) => new Criterion { Name = c.Name, Weight = c.Weight, DisplayOrder = i + 1 }).ToList();

    private static IEnumerable<ScoreEntry> Scores(List<Criterion> cs, params decimal[] values) =>
        cs.Zip(values, (c, v) => new ScoreEntry(c.Id, v, null));

    [Fact]
    public void CalculateTotal_UsesWeights()
    {
        var cs = Criteria();
        var ev = new Evaluation();
        ev.SetScores(cs, Scores(cs, 10, 8, 6, 9, 7));

        ev.CalculateTotal(cs).Should().Be(8.1m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(10.01)]
    public void SetScores_OutOfRange_ThrowsInvalid(double score)
    {
        var cs = Criteria();
        var ev = new Evaluation();

        var act = () => ev.SetScores(cs, new[] { new ScoreEntry(cs[0].Id, (decimal)score, null) });

        act.Should().Throw<EvaluationRuleException>().Which.Rule.Should().Be(EvaluationRule.Invalid);
    }

    [Fact]
    public void SetScores_UnknownCriterion_ThrowsInvalid()
    {
        var ev = new Evaluation();

        var act = () => ev.SetScores(Criteria(), new[] { new ScoreEntry(Guid.NewGuid(), 5, null) });

        act.Should().Throw<EvaluationRuleException>().Which.Rule.Should().Be(EvaluationRule.Invalid);
    }

    [Fact]
    public void SetScores_ExistingDetail_UpdatesWithoutAdding()
    {
        var cs = Criteria();
        var ev = new Evaluation();
        ev.SetScores(cs, Scores(cs, 5));

        var added = ev.SetScores(cs, Scores(cs, 9));

        added.Should().BeEmpty();
        ev.Details.Should().ContainSingle().Which.Score.Should().Be(9);
    }

    [Fact]
    public void Publish_MissingCriteria_ThrowsInvalid()
    {
        var cs = Criteria();
        var ev = new Evaluation();
        ev.SetScores(cs, Scores(cs, 10, 8, 6));

        var act = () => ev.Publish(cs);

        act.Should().Throw<EvaluationRuleException>().Which.Rule.Should().Be(EvaluationRule.Invalid);
        ev.Status.Should().Be(EvaluationStatus.Draft);
    }

    [Fact]
    public void Publish_Full_SetsPublished()
    {
        var cs = Criteria();
        var ev = new Evaluation();
        ev.SetScores(cs, Scores(cs, 10, 8, 6, 9, 7));

        ev.Publish(cs);

        ev.Status.Should().Be(EvaluationStatus.Published);
        ev.SubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public void Edit_AfterPublished_ThrowsConflict()
    {
        var cs = Criteria();
        var ev = new Evaluation();
        ev.SetScores(cs, Scores(cs, 10, 8, 6, 9, 7));
        ev.Publish(cs);

        var act = () => ev.SetScores(cs, Scores(cs, 1));

        act.Should().Throw<EvaluationRuleException>().Which.Rule.Should().Be(EvaluationRule.Conflict);
    }
}
