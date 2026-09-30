using InternFlow.Domain.Enums;
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

        (TaskStatusValue.InProgress, TaskStatusValue.Review, UserRole.Intern),
        (TaskStatusValue.InProgress, TaskStatusValue.Review, UserRole.Mentor),
        (TaskStatusValue.InProgress, TaskStatusValue.Review, UserRole.Admin),

        (TaskStatusValue.Review, TaskStatusValue.Done, UserRole.Mentor),
        (TaskStatusValue.Review, TaskStatusValue.Done, UserRole.Admin),

        (TaskStatusValue.Review, TaskStatusValue.InProgress, UserRole.Mentor),
        (TaskStatusValue.Review, TaskStatusValue.InProgress, UserRole.Admin),

        (TaskStatusValue.Todo, TaskStatusValue.Cancelled, UserRole.Mentor),
        (TaskStatusValue.Todo, TaskStatusValue.Cancelled, UserRole.Admin),

        (TaskStatusValue.InProgress, TaskStatusValue.Cancelled, UserRole.Mentor),
        (TaskStatusValue.InProgress, TaskStatusValue.Cancelled, UserRole.Admin),

        (TaskStatusValue.Review, TaskStatusValue.Cancelled, UserRole.Mentor),
        (TaskStatusValue.Review, TaskStatusValue.Cancelled, UserRole.Admin),
    };

    /// <summary>
    /// Test bao phủ toàn bộ: duyệt hết mọi tổ hợp (5 trạng thái x 5 trạng thái x 3 vai trò
    /// = 75 trường hợp) và so với bảng kỳ vọng ở trên. Đây là bằng chứng "phủ hết ma trận"
    /// theo đúng tiêu chí nghiệm thu của #T6, không chỉ test vài ca lẻ tẻ.
    /// </summary>
    [Fact]
    public void CanTransition_khop_voi_toan_bo_ma_tran_ky_vong()
    {
        var allStatuses = System.Enum.GetValues<TaskStatusValue>();
        var allRoles = System.Enum.GetValues<UserRole>();
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

    // ----- Các ca tiêu biểu, viết riêng để khi test fail thì tên test nói ngay
    //       vấn đề nghiệp vụ, không phải tự suy luận từ vòng lặp ở trên -----

    [Fact]
    public void Intern_duoc_tu_chuyen_Todo_sang_InProgress()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.Todo, TaskStatusValue.InProgress, UserRole.Intern));
    }

    [Fact]
    public void Intern_duoc_tu_chuyen_InProgress_sang_Review()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.InProgress, TaskStatusValue.Review, UserRole.Intern));
    }

    [Fact]
    public void Intern_KHONG_duoc_tu_duyet_Review_sang_Done()
    {
        Assert.False(TaskWorkflow.CanTransition(
            TaskStatusValue.Review, TaskStatusValue.Done, UserRole.Intern));
    }

    [Fact]
    public void Mentor_duoc_duyet_Review_sang_Done()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.Review, TaskStatusValue.Done, UserRole.Mentor));
    }

    [Fact]
    public void Mentor_duoc_tra_lai_Review_ve_InProgress()
    {
        Assert.True(TaskWorkflow.CanTransition(
            TaskStatusValue.Review, TaskStatusValue.InProgress, UserRole.Mentor));
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
        var allStatuses = System.Enum.GetValues<TaskStatusValue>();
        var allRoles = System.Enum.GetValues<UserRole>();

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
            TaskStatusValue.Review, TaskStatusValue.Cancelled, UserRole.Intern));
    }

    // ----- Test cho TryTransition: đảm bảo phân biệt đúng hai loại lỗi
    //       để tầng API map đúng 409 (sai luồng) và 403 (sai quyền) -----

    [Fact]
    public void TryTransition_thanh_cong_khi_dung_luong_va_dung_quyen()
    {
        var ketQua = TaskWorkflow.TryTransition(
            TaskStatusValue.Review, TaskStatusValue.Done, UserRole.Mentor);

        Assert.True(ketQua.IsSuccess);
        Assert.Equal(TransitionFailureReason.None, ketQua.FailureReason);
    }

    [Fact]
    public void TryTransition_bao_loi_InvalidTransition_khi_sai_luong()
    {
        // Todo -> Done không tồn tại trong quy trình, bất kể vai trò nào gọi
        var ketQua = TaskWorkflow.TryTransition(
            TaskStatusValue.Todo, TaskStatusValue.Done, UserRole.Admin);

        Assert.False(ketQua.IsSuccess);
        Assert.Equal(TransitionFailureReason.InvalidTransition, ketQua.FailureReason);
        Assert.NotNull(ketQua.ErrorMessage);
    }

    [Fact]
    public void TryTransition_bao_loi_Forbidden_khi_dung_luong_nhung_sai_quyen()
    {
        // Review -> Done tồn tại, nhưng Intern không được thực hiện
        var ketQua = TaskWorkflow.TryTransition(
            TaskStatusValue.Review, TaskStatusValue.Done, UserRole.Intern);

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
    public void GetAvailableNextStatuses_tra_ba_lua_chon_cho_Mentor_tu_Review()
    {
        var ketQua = TaskWorkflow.GetAvailableNextStatuses(TaskStatusValue.Review, UserRole.Mentor);

        Assert.Equal(3, ketQua.Count);
        Assert.Contains(TaskStatusValue.Done, ketQua);
        Assert.Contains(TaskStatusValue.InProgress, ketQua);
        Assert.Contains(TaskStatusValue.Cancelled, ketQua);
    }
}
