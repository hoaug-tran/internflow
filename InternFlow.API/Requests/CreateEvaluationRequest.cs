using InternFlow.Application.Features.Evaluations.DTOs;

namespace InternFlow.API.Requests;

/// <summary>Dữ liệu tạo phiếu đánh giá.</summary>
public class CreateEvaluationRequest
{
    /// <summary>Id kỳ đánh giá (lấy từ GET /api/evaluation-periods).</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid EvaluationPeriodId { get; set; }

    /// <summary>Id intern được đánh giá.</summary>
    /// <example>9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d</example>
    public Guid InternId { get; set; }

    /// <summary>Nhận xét chung.</summary>
    /// <example>Hoàn thành tốt công việc được giao.</example>
    public string? OverallComment { get; set; }

    /// <summary>Điểm từng tiêu chí (có thể chấm thiếu khi còn Draft).</summary>
    public List<ScoreInput> Scores { get; set; } = new();
}
