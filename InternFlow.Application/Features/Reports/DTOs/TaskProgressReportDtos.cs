namespace InternFlow.Application.Features.Reports.DTOs;

/// <summary>Tiến độ task của một intern.</summary>
public class TaskProgressRowDto
{
    /// <summary>Id intern.</summary>
    public Guid InternId { get; set; }

    /// <summary>Họ tên intern.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Tổng số task được giao.</summary>
    public int TotalTasks { get; set; }

    public int Todo { get; set; }
    public int InProgress { get; set; }
    public int InReview { get; set; }
    public int Done { get; set; }
    public int Cancelled { get; set; }

    /// <summary>Số task quá hạn (chưa Done/Cancelled và DueDate đã qua).</summary>
    public int Overdue { get; set; }

    /// <summary>Tỷ lệ hoàn thành (%) = Done / (Total − Cancelled).</summary>
    public decimal CompletionRate { get; set; }

    /// <summary>Tổng giờ làm (không tính worklog đã xóa).</summary>
    public decimal TotalHours { get; set; }
}
