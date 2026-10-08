namespace InternFlow.Domain.Evaluations;

// Toàn bộ luật nghiệp vụ của phiếu đánh giá nằm ở đây
public static class EvaluationRules
{
    public const decimal MinScore = 0m;
    public const decimal MaxScore = 10m;
    public const decimal RequiredTotalWeight = 100m;

    // Phiếu đã Published thì không được sửa -> 409
    public static void EnsureEditable(this Evaluation evaluation)
    {
        if (evaluation.Status == EvaluationStatus.Published)
            throw new EvaluationRuleException(EvaluationRule.Conflict,
                "Phiếu đánh giá đã Published, không thể chỉnh sửa.");
    }

    // Ghi điểm cho các tiêu chí. Trả về danh sách detail MỚI tạo
    public static IReadOnlyList<EvaluationDetail> SetScores(
        this Evaluation evaluation,
        IReadOnlyCollection<Criterion> activeCriteria,
        IEnumerable<ScoreEntry> scores)
    {
        evaluation.EnsureEditable();

        var list = scores.ToList();
        ValidateScores(activeCriteria, list);

        var added = new List<EvaluationDetail>();
        foreach (var s in list)
        {
            var detail = evaluation.Details.FirstOrDefault(d => d.CriterionId == s.CriterionId);
            if (detail is null)
            {
                detail = new EvaluationDetail
                {
                    EvaluationId = evaluation.Id,
                    CriterionId = s.CriterionId,
                    Score = s.Score,
                    Comment = s.Comment
                };
                evaluation.Details.Add(detail);
                added.Add(detail);
            }
            else
            {
                detail.Score = s.Score;
                detail.Comment = s.Comment;
            }
        }

        evaluation.UpdatedAt = DateTime.UtcNow;
        return added;
    }

    // Điểm tổng = Σ (điểm × trọng số / 100), làm tròn 2 số
    public static decimal CalculateTotal(this Evaluation evaluation, IEnumerable<Criterion> criteria)
    {
        var weights = criteria.Where(c => c.IsActive).ToDictionary(c => c.Id, c => c.Weight);

        var total = evaluation.Details
            .Where(d => weights.ContainsKey(d.CriterionId))
            .Sum(d => d.Score * weights[d.CriterionId] / 100m);

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    // Publish: phải chấm đủ mọi tiêu chí -> thiếu thì 400
    public static void Publish(this Evaluation evaluation, IReadOnlyCollection<Criterion> activeCriteria)
    {
        evaluation.EnsureEditable();

        if (activeCriteria.Count == 0)
            throw Invalid("Kỳ đánh giá chưa có tiêu chí.");

        var totalWeight = activeCriteria.Sum(c => c.Weight);
        if (totalWeight != RequiredTotalWeight)
            throw Invalid($"Tổng trọng số tiêu chí là {totalWeight}, cần đúng 100.");

        var scoredIds = evaluation.Details.Select(d => d.CriterionId).ToHashSet();
        var missing = activeCriteria.Where(c => !scoredIds.Contains(c.Id)).Select(c => c.Name).ToList();
        if (missing.Count > 0)
            throw Invalid($"Thiếu điểm cho {missing.Count} tiêu chí: {string.Join(", ", missing)}.");

        evaluation.Status = EvaluationStatus.Published;
        evaluation.SubmittedAt = DateTime.UtcNow;
        evaluation.UpdatedAt = evaluation.SubmittedAt;
    }

    // Kiểm tra điểm hợp lệ -> sai thì 400
    private static void ValidateScores(IReadOnlyCollection<Criterion> activeCriteria, List<ScoreEntry> scores)
    {
        if (scores.GroupBy(s => s.CriterionId).Any(g => g.Count() > 1))
            throw Invalid("Một tiêu chí bị chấm trùng trong cùng yêu cầu.");

        var validIds = activeCriteria.Select(c => c.Id).ToHashSet();

        foreach (var s in scores)
        {
            if (!validIds.Contains(s.CriterionId))
                throw Invalid($"Tiêu chí {s.CriterionId} không thuộc kỳ đánh giá hoặc đã ngừng sử dụng.");

            if (s.Score < MinScore || s.Score > MaxScore)
                throw Invalid($"Điểm phải trong khoảng {MinScore}–{MaxScore} (nhận {s.Score}).");

            if (decimal.Round(s.Score, 2) != s.Score)
                throw Invalid("Điểm chỉ được tối đa 2 chữ số thập phân.");
        }
    }

    private static EvaluationRuleException Invalid(string message) =>
        new(EvaluationRule.Invalid, message);
}
