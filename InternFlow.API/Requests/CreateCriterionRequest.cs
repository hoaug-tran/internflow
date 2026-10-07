namespace InternFlow.API.Requests;

/// <summary>Dữ liệu tạo tiêu chí đánh giá mới cho một kỳ.</summary>
public class CreateCriterionRequest
{
    /// <summary>Tên tiêu chí đánh giá.</summary>
    /// <example>Thái độ làm việc</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Mô tả chi tiết tiêu chí (tuỳ chọn).</summary>
    /// <example>Đánh giá tinh thần, sự chủ động và trách nhiệm trong công việc</example>
    public string? Description { get; set; }

    /// <summary>Trọng số tiêu chí (%). Tổng các tiêu chí active trong kỳ không được vượt 100.</summary>
    /// <example>20</example>
    public decimal Weight { get; set; }

    /// <summary>Thứ tự hiển thị trong danh sách tiêu chí.</summary>
    /// <example>1</example>
    public int DisplayOrder { get; set; }
}
