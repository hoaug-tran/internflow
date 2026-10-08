using InternFlow.Application.Features.Evaluations.DTOs;

namespace InternFlow.API.Requests;

/// <summary>Dữ liệu sửa phiếu đánh giá.</summary>
public class UpdateEvaluationRequest
{
    /// <summary>Nhận xét chung.</summary>
    /// <example>Cần cải thiện kỹ năng làm việc nhóm.</example>
    public string? OverallComment { get; set; }

    /// <summary>Điểm từng tiêu chí. Tiêu chí nào đã có điểm sẽ được ghi đè.</summary>
    public List<ScoreInput> Scores { get; set; } = new();
}
