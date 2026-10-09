namespace InternFlow.Application.Features.Reports.DTOs;

/// <summary>Khối lượng công việc của một intern.</summary>
public class InternWorkloadRowDto
{
    /// <summary>Id intern.</summary>
    public Guid InternId { get; set; }

    /// <summary>Họ tên intern.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Tổng giờ làm (không tính worklog đã xóa).</summary>
    public decimal TotalHours { get; set; }

    /// <summary>Số lần ghi worklog.</summary>
    public int WorkLogCount { get; set; }

    /// <summary>Số ngày có làm việc (WorkDate khác nhau).</summary>
    public int WorkDays { get; set; }

    /// <summary>Số task khác nhau đã ghi giờ.</summary>
    public int TaskCount { get; set; }

    /// <summary>Giờ trung bình mỗi ngày làm việc.</summary>
    public decimal AvgHoursPerWorkDay { get; set; }
}

/// <summary>Khối lượng công việc của các intern thuộc một mentor.</summary>
public class MentorWorkloadRowDto
{
    /// <summary>Id mentor.</summary>
    public Guid MentorId { get; set; }

    /// <summary>Họ tên mentor.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Số intern đang phụ trách.</summary>
    public int InternCount { get; set; }

    /// <summary>Tổng giờ làm của các intern đang phụ trách.</summary>
    public decimal TotalHours { get; set; }

    /// <summary>Giờ trung bình mỗi intern.</summary>
    public decimal AvgHoursPerIntern { get; set; }
}
