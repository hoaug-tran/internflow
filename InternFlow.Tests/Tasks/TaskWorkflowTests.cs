using TaskStatusValue = InternFlow.Domain.Projects.TaskStatus;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Tasks;
using Xunit;

namespace InternFlow.Domain.Tests.Tasks;

public class TaskWorkflowTests
{
    /// <summary>
    /// Danh sách TOÀN BỘ các bộ ba (from, to, role) được phép — chép lại đúng
    /// bảng quy tắc trong TaskWorkflow, nhưng viết tay độc lập (không gọi lại
    /// AllowedTransitions) để test thực sự kiểm tra hành vi, không phải kiểm
    /// tra chính nó.
    /// </summary>
    private static readonly HashSet<(TaskStatusValue From, TaskStatusValue To, UserRole Role)> ExpectedAllowed = new()
    {
        (TaskStatusValue.Todo, TaskStatusValue.InProgress, UserRole.Intern),
        (TaskStatusValue.Todo, TaskStatusValue.InProgress, UserRole.Mentor),
        (TaskStatusValue.Todo, TaskStatusValue.InProgress, UserRole.Admin),

        (TaskStatusValue.InProgress, TaskStatusValue.InReview, UserRole.Intern),
        (TaskStatusValue.InProgress, TaskStatusValue.InReview, UserRole.Mentor),
        (TaskStatusValue.InProgress, TaskStatusValue.InReview, UserRole.Admin),

        (TaskStatusValue.InReview, TaskStatusValue.Done, UserRole.Mentor),
        (TaskStatusValue.InReview, TaskStatusValue.Done, UserRole.Admin),

        (TaskStatusValue.InReview, TaskStatusValue.InProgress, UserRole.Mentor),
        (TaskStatusValue.InReview, TaskStatusValue.InProgress, UserRole.Admin),

        (TaskStatusValue.Todo, TaskStatusValue.Cancelled, UserRole.Mentor),
        (TaskStatusValue.Todo, TaskStatusValue.Cancelled, UserRole.Admin),

        (TaskStatusValue.InProgress, TaskStatusValue.Cancelled, UserRole.Mentor),
        (TaskStatusValue.InProgress, TaskStatusValue.Cancelled, UserRole.Admin),

        (TaskStatusValue.InReview, TaskStatusValue.Cancelled, UserRole.Mentor),
        (TaskStatusValue.InReview, TaskStatusValue.Cancelled, UserRole.Admin),
    };

    [Fact]
    public void CanTransition_khop_voi_toan_bo_ma_tran_ky_vong()
    {
        var allStatuses = Enum.GetValues<TaskStatusValue>();
        var allRoles = Enum.GetValues<UserRole>();
        var saiLech = new List<string>();

        foreach (var from in allStatuses)
        {
            foreach (var to in allStatuses)
            {
                foreach (var role in allRoles)
                {
                    var kyVong = ExpectedAllowed.Contains((from, to, role));
                    var thucTe = TaskWorkflow.CanTransition(from, to, role);

                    if (kyVong != thucTe)
                    {
                        saiLech.Add(
                            $"{from} -> {to} với vai trò {role}: kỳ vọng {kyVong}, thực tế {thucTe}");
                    }
                }
            }
        }

        Assert.True(
            saiLech.Count == 0,
            "Có sai lệch so với ma trận kỳ vọng:" + Environment.NewLine + string.Join(Environment.NewLine, saiLech));
    }

    [Fact]
    public void Ma_tran_co_dung_7_buoc_chuyen_hop_le()
    {
        var allStatuses = Enum.GetValues<TaskStatusValue>();
        var transitions = new List<(TaskStatusValue From, TaskStatusValue To)>();

        foreach (var from in allStatuses)
        {
            foreach (var to in allStatuses)
            {
                if (TaskWorkflow.IsKnownTransition(from, to))
                    transitions.Add((from, to));
            }
        }

        Assert.Equal(7, transitions.Count);
    }


    [Theory]
    [InlineData(TaskStatusValue.Todo, TaskStatusValue.InProgress, true)]
    [InlineData(TaskStatusValue.InProgress, TaskStatusValue.InReview, true)]
    [InlineData(TaskStatusValue.InReview, TaskStatusValue.Done, true)]
    [InlineData(TaskStatusValue.InReview, TaskStatusValue.InProgress, true)]
    [InlineData(TaskStatusValue.Todo, TaskStatusValue.Cancelled, true)]
    [InlineData(TaskStatusValue.InProgress, TaskStatusValue.Cancelled, true)]
    [InlineData(TaskStatusValue.InReview, TaskStatusValue.Cancelled, true)]
    [InlineData(TaskStatusValue.Todo, TaskStatusValue.Done, false)]
    [InlineData(TaskStatusValue.Todo, TaskStatusValue.InReview, false)]
    [InlineData(TaskStatusValue.Done, TaskStatusValue.Todo, false)]
    [InlineData(TaskStatusValue.Cancelled, TaskStatusValue.Todo, false)]
    [InlineData(TaskStatusValue.InProgress, TaskStatusValue.Todo, false)]
    public void IsKnownTransition_tra_ve_dung(
        TaskStatusValue from, TaskStatusValue to, bool expected)
    {
        Assert.Equal(expected, TaskWorkflow.IsKnownTransition(from, to));
    }


    [Fact]
    public void Intern_duoc_tu_chuyen_Todo_sang_InProgress()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.Todo, TaskStatusValue.InProgress, UserRole.Intern));
    }

    [Fact]
    public void Intern_duoc_tu_chuyen_InProgress_sang_InReview()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.InProgress, TaskStatusValue.InReview, UserRole.Intern));
    }

    [Fact]
    public void Intern_KHONG_duoc_tu_duyet_InReview_sang_Done()
    {
        Assert.False(TaskWorkflow.CanTransition(
            TaskStatusValue.InReview, TaskStatusValue.Done, UserRole.Intern));
    }

    [Fact]
    public void Mentor_duoc_duyet_InReview_sang_Done()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.InReview, TaskStatusValue.Done, UserRole.Mentor));
    }

    [Fact]
    public void Mentor_duoc_tra_lai_InReview_ve_InProgress()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.InReview, TaskStatusValue.InProgress, UserRole.Mentor));
    }

    [Fact]
    public void Khong_duoc_bo_qua_buoc_tu_Todo_thang_len_Done()
    {
        Assert.False(TaskWorkflow.CanTransition(
            TaskStatusValue.Todo, TaskStatusValue.Done, UserRole.Admin));
    }

    [Theory]
    [InlineData(TaskStatusValue.Done)]
    [InlineData(TaskStatusValue.Cancelled)]
    public void Trang_thai_cuoi_khong_chuyen_di_dau_duoc_nua(TaskStatusValue trangThaiCuoi)
    {
        var allStatuses = Enum.GetValues<TaskStatusValue>();
        var allRoles = Enum.GetValues<UserRole>();

        foreach (var to in allStatuses)
        {
            foreach (var role in allRoles)
            {
                Assert.False(
                    TaskWorkflow.CanTransition(trangThaiCuoi, to, role),
                    $"{trangThaiCuoi} không được chuyển sang {to} dù với vai trò {role}");
            }
        }
    }

    [Fact]
    public void Intern_khong_duoc_huy_cong_viec()
    {
        Assert.False(TaskWorkflow.CanTransition(
            TaskStatusValue.Todo, TaskStatusValue.Cancelled, UserRole.Intern));
        Assert.False(TaskWorkflow.CanTransition(
            TaskStatusValue.InProgress, TaskStatusValue.Cancelled, UserRole.Intern));
        Assert.False(TaskWorkflow.CanTransition(
            TaskStatusValue.InReview, TaskStatusValue.Cancelled, UserRole.Intern));
    }


    [Fact]
    public void TryTransition_thanh_cong_khi_dung_luong_va_dung_quyen()
    {
        var ketQua = TaskWorkflow.TryTransition(
            TaskStatusValue.InReview, TaskStatusValue.Done, UserRole.Mentor);

        Assert.True(ketQua.IsSuccess);
        Assert.Equal(TransitionFailureReason.None, ketQua.FailureReason);
        Assert.Null(ketQua.ErrorMessage);
    }

    [Fact]
    public void TryTransition_bao_loi_InvalidTransition_khi_sai_luong()
    {
        var ketQua = TaskWorkflow.TryTransition(
            TaskStatusValue.Todo, TaskStatusValue.Done, UserRole.Admin);

        Assert.False(ketQua.IsSuccess);
        Assert.Equal(TransitionFailureReason.InvalidTransition, ketQua.FailureReason);
        Assert.NotNull(ketQua.ErrorMessage);
    }

    [Fact]
    public void TryTransition_bao_loi_Forbidden_khi_dung_luong_nhung_sai_quyen()
    {
        var ketQua = TaskWorkflow.TryTransition(
            TaskStatusValue.InReview, TaskStatusValue.Done, UserRole.Intern);

        Assert.False(ketQua.IsSuccess);
        Assert.Equal(TransitionFailureReason.Forbidden, ketQua.FailureReason);
        Assert.NotNull(ketQua.ErrorMessage);
    }


    [Fact]
    public void GetAvailableNextStatuses_tra_dung_danh_sach_cho_Intern_tu_Todo()
    {
        var ketQua = TaskWorkflow.GetAvailableNextStatuses(TaskStatusValue.Todo, UserRole.Intern);

        Assert.Single(ketQua);
        Assert.Contains(TaskStatusValue.InProgress, ketQua);
    }

    [Fact]
    public void GetAvailableNextStatuses_tra_ba_lua_chon_cho_Mentor_tu_InReview()
    {
        var ketQua = TaskWorkflow.GetAvailableNextStatuses(TaskStatusValue.InReview, UserRole.Mentor);

        Assert.Equal(3, ketQua.Count);
        Assert.Contains(TaskStatusValue.Done, ketQua);
        Assert.Contains(TaskStatusValue.InProgress, ketQua);
        Assert.Contains(TaskStatusValue.Cancelled, ketQua);
    }

    [Fact]
    public void GetAvailableNextStatuses_tra_rong_cho_Intern_tu_Done()
    {
        var ketQua = TaskWorkflow.GetAvailableNextStatuses(TaskStatusValue.Done, UserRole.Intern);

        Assert.Empty(ketQua);
    }
}
