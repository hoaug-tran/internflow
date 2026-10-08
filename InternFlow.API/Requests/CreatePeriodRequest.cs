namespace InternFlow.API.Requests;

/// <summary>Dữ liệu tạo kỳ đánh giá mới cho một đợt thực tập.</summary>
public class CreatePeriodRequest
{
    /// <summary>Id đợt thực tập mà kỳ đánh giá thuộc về.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid InternshipId { get; set; }

    /// <summary>Tên kỳ đánh giá.</summary>
    /// <example>Đánh giá giữa kỳ</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Mô tả ngắn về kỳ đánh giá (tuỳ chọn).</summary>
    /// <example>Kỳ đánh giá giữa đợt thực tập</example>
    public string? Description { get; set; }

    /// <summary>Ngày bắt đầu kỳ đánh giá.</summary>
    /// <example>2026-10-01</example>
    public DateOnly StartDate { get; set; }

    /// <summary>Ngày kết thúc kỳ đánh giá (phải &gt;= StartDate).</summary>
    /// <example>2026-10-31</example>
    public DateOnly EndDate { get; set; }
}
