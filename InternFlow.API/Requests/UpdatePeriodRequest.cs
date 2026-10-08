namespace InternFlow.API.Requests;

/// <summary>Dữ liệu cập nhật kỳ đánh giá (không thay đổi đợt thực tập).</summary>
public class UpdatePeriodRequest
{
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
