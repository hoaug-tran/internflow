# HƯỚNG DẪN PHÂN QUYỀN & DATA SCOPE AUTHORIZATION (INTERNFLOW)

> **Tài liệu bàn giao kỹ thuật từ:** Hiếu (Module Identity & Internships)  
> **Dành cho:** Tú (Module Projects, Tasks & Worklogs) & Quang (Module Evaluations & Reports)  
> **Mục đích:** Thống nhất ma trận quyền, cơ chế kiểm soát quyền theo vai trò lẫn phạm vi dữ liệu (Data Scope), và chuẩn hóa mã lỗi HTTP theo yêu cầu đề bài.

---

## 1. Cơ chế Xác thực & Trích xuất Danh tính

Hệ thống sử dụng **JWT Bearer Token** với các Claims định danh chuẩn:
* `ClaimTypes.NameIdentifier` / `sub`: `UserId` (Guid)
* `ClaimTypes.Email` / `email`: Email người dùng
* `ClaimTypes.Role` / `role`: `Admin` | `Mentor` | `Intern`

### 1.1. Cách lấy thông tin User trong MediatR Handler
Thay vì trích xuất thủ công từ `HttpContext` ở Controller, hãy inject **`ICurrentUserService`** trực tiếp vào các Command/Query Handler ở tầng Application:

```csharp
using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;

public class CreateWorkLogCommandHandler : IRequestHandler<CreateWorkLogCommand, ApiResponse<long>>
{
    private readonly IInternFlowDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateWorkLogCommandHandler(
        IInternFlowDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<long>> Handle(CreateWorkLogCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực (Đăng nhập)
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == null)
        {
            return ApiResponse<long>.Fail("Yêu cầu đăng nhập.");
        }

        var currentUserId = _currentUser.UserId.Value;
        var roleString = _currentUser.Role; // "Admin", "Mentor", "Intern"
        var isIntern = _currentUser.IsInRole(UserRole.Intern.ToString());

        // ... Xử lý nghiệp vụ và kiểm tra Data Scope
    }
}
```

---

## 2. Ma trận Phân quyền theo Vai trò (Role Matrix)

| Phân hệ / Tài nguyên | Thao tác (Endpoint / Hành động) | Admin | Mentor | Intern |
| :--- | :--- | :---: | :---: | :---: |
| **Auth** | Đăng nhập, Refresh Token, Thu hồi Token, Xem thông tin cá nhân (`/me`) | ✅ | ✅ | ✅ |
| **Internships** | Tạo đợt thực tập, Đổi trạng thái đợt (`Planned` $\rightarrow$ `Active` $\rightarrow$ `Closed`) | ✅ | ❌ | ❌ |
| | Thêm Mentor / Intern vào đợt thực tập | ✅ | ❌ | ❌ |
| | Cập nhật tiến độ Intern trong đợt (`InProgress`, `Completed`, `DroppedOut`, `Failed`) | ✅ | ✅ | ❌ |
| | Xem danh sách & chi tiết đợt thực tập | ✅ | ✅ | ✅ |
| **Mentor Assignment**| Phân công Mentor cho Intern (ràng buộc `MaxMentees`) | ✅ | ❌ | ❌ |
| | Kết thúc phân công Mentor (`EndMentorAssignment`) | ✅ | ❌ | ❌ |
| | Xem lịch sử phân công Mentor | ✅ | ✅ | ❌ (Chỉ xem phân công của chính mình) |
| **Projects** | Tạo dự án, thêm/xóa thành viên dự án | ✅ | ✅ (Chỉ dự án mình phụ trách) | ❌ |
| | Xem danh sách dự án | ✅ (Toàn bộ) | ✅ (Dự án tham gia) | ✅ (Dự án tham gia) |
| **Tasks** | Tạo task, giao task cho thành viên dự án | ✅ | ✅ (Dự án tham gia) | ❌ |
| | Chuyển trạng thái `Todo` $\rightarrow$ `InProgress` $\rightarrow$ `InReview` | ✅ | ✅ | ✅ (Chỉ task của mình) |
| | Duyệt task sang `Done` hoặc `Cancelled` | ✅ | ✅ (Mentor phụ trách) | ❌ **(BỊ CẤM)** |
| **Worklogs** | Tạo / Sửa worklog gắn với task | ❌ | ❌ | ✅ (Chỉ task được giao) |
| | Xem danh sách worklog | ✅ (Toàn bộ) | ✅ (Intern mình phụ trách / Dự án mình) | ✅ (Chỉ của bản thân) |
| **Evaluations** | Tạo kỳ đánh giá, thiết lập tiêu chí & thang điểm | ✅ | ❌ | ❌ |
| | Lập phiếu đánh giá & chấm điểm Intern | ❌ | ✅ (Chỉ Intern mình phụ trách) | ❌ |
| | Xem kết quả đánh giá | ✅ (Toàn bộ) | ✅ (Intern mình phụ trách) | ✅ (Chỉ kết quả công bố của mình) |
| **Reports (Báo cáo)** | Báo cáo tiến độ task theo dự án / đợt (Dapper + SP) | ✅ | ✅ (Dự án mình tham gia) | ❌ |
| | Báo cáo khối lượng công việc theo mentor/intern (Dapper + SP) | ✅ | ✅ (Intern mình phụ trách) | ❌ |

---

## 3. Quy tắc Kiểm tra Phạm vi Dữ liệu (Data Scope Authorization)

Đề bài nêu rõ nguyên tắc: **"Biết một ID hợp lệ không đồng nghĩa được đọc hoặc sửa tài nguyên đó"**. Dưới đây là các quy tắc bắt buộc cần cài đặt trong Handler:

### 3.1. Phân hệ Task & Worklog (Tú phụ trách)

#### A. Quản lý luồng trạng thái Task (Tận dụng `TaskWorkflow` có sẵn)
Tú đã xây dựng class máy trạng thái `TaskWorkflow` tại `InternFlow.Domain.Tasks`. Hãy gọi trực tiếp phương thức `TryTransition`:
```csharp
using InternFlow.Domain.Tasks;

// Trong UpdateTaskStatusCommandHandler:
var result = TaskWorkflow.TryTransition(task.Status, request.NewStatus, userRole);

if (!result.IsSuccess)
{
    // Nếu là bước chuyển không có trong quy trình -> trả về lỗi 409 Conflict
    // Nếu bước chuyển có thật nhưng vai trò không được phép (Intern cố tự duyệt Done) -> trả về 403 Forbidden
    return ApiResponse<bool>.Fail(result.ErrorMessage);
}
```

#### B. Quyền sở hữu Worklog:
1. **Khi Intern tạo Worklog (`CreateWorkLogCommand`):**
   - Task bắt buộc phải do chính intern đó nhận: `task.AssigneeId == _currentUser.UserId`.
   - Task không được ở trạng thái `Cancelled` hoặc `Done`.
2. **Khi Intern sửa Worklog (`UpdateWorkLogCommand`):**
   - Phải kiểm tra: `workLog.InternId == _currentUser.UserId`.
   - Tuyệt đối không cho phép sửa worklog của intern khác.
3. **Khi Mentor xem Worklog:**
   - Mentor phải đang trực tiếp hướng dẫn Intern đó trong đợt (`MentorAssignments` có `EndedAt == null`), HOẶC cùng tham gia vào Project chứa Task của worklog đó.

---

### 3.2. Phân hệ Evaluation (Quang phụ trách)

#### A. Mentor A không được đánh giá Intern của Mentor B:
Khi Mentor submit đánh giá cho một intern trong kỳ (`CreateEvaluationCommand`), phải xác minh quan hệ phân công thực tế:
```csharp
var isAssignedMentor = await _context.MentorAssignments
    .AnyAsync(ma => ma.InternshipId == request.InternshipId 
                 && ma.InternId == request.InternId 
                 && ma.MentorId == _currentUser.UserId 
                 && ma.EndedAt == null, cancellationToken);

if (!isAssignedMentor && _currentUser.Role != "Admin")
{
    return ApiResponse<long>.Fail("Bạn không phải là Mentor đang phụ trách thực tập sinh này.");
}
```

#### B. Ẩn kết quả đánh giá nháp (Draft) đối với Intern:
* Nếu Intern xem kết quả đánh giá (`GetEvaluationByInternQuery`), chỉ hiển thị các bản ghi có `Status == EvaluationStatus.Published` hoặc `EvaluationStatus.Completed`.
* Các bản ghi ở trạng thái `Draft` tuyệt đối không trả về cho Intern.

---

### 3.3. Phân hệ Báo cáo (Reports - Dapper & Stored Procedure của Quang)

Dữ liệu đầu ra của Stored Procedure phải được lọc theo phạm vi người gọi:
1. **Báo cáo tiến độ dự án:**
   - **Admin:** Xem báo cáo của mọi dự án.
   - **Mentor:** Chỉ truyền `ProjectId` của các dự án mà mình là thành viên (`ProjectMembers`) vào Stored Procedure. Nếu Mentor cố tình truyền `ProjectId` lạ, trả về **403 Forbidden** hoặc **404 Not Found**.
   - **Intern:** Không có quyền truy cập endpoint báo cáo này (**403 Forbidden**).
2. **Báo cáo khối lượng / kết quả Intern:**
   - **Mentor:** Chỉ được xem dữ liệu của các Intern mà mình có phân công đang hiệu lực (`MentorId == _currentUser.UserId`).

---

## 4. Bảng Quy ước Mã lỗi HTTP (HTTP Status Codes - Tiêu chí Mục 8)

Đề bài yêu cầu hệ thống phải phản hồi đúng mã lỗi HTTP theo từng ngữ cảnh:

| Mã lỗi HTTP | Tên mã lỗi | Khi nào áp dụng trong phân quyền & nghiệp vụ |
| :---: | :--- | :--- |
| **`400`** | **Bad Request** | Dữ liệu đầu vào sai cú pháp hoặc vi phạm validation (ví dụ: ngày kết thúc trước ngày bắt đầu, vượt quá giới hạn phân trang). |
| **`401`** | **Unauthorized** | Client không gửi Header `Authorization`, token sai chữ ký bí mật, hoặc Refresh Token đã bị thu hồi/hết hạn. |
| **`403`** | **Forbidden** | Đã xác thực thành công nhưng **không đủ thẩm quyền** (ví dụ: Intern gọi API tạo đợt thực tập, Intern tự duyệt task sang `Done`, Mentor A cố chấm điểm cho intern của Mentor B). |
| **`404`** | **Not Found** | Tài nguyên không tồn tại, HOẶC **cố tình trả về 404 để không tiết lộ sự tồn tại của dữ liệu** khi người dùng không có quyền truy cập (chống brute-force dò quét ID). |
| **`409`** | **Conflict** | Xung đột trạng thái nghiệp vụ (ví dụ: chuyển task từ `Todo` nhảy cóc sang `Done`, đợt thực tập đã đóng không cho thêm thành viên). |

---

## 5. Tài khoản Demo sẵn có trong Database Seeder để Kiểm thử

Mật khẩu mặc định cho toàn bộ tài khoản demo là: **`Password123!`** (đã được mã hóa BCrypt an toàn).

### A. Dữ liệu Seeder Tiêu chuẩn (`DatabaseSeeder.cs`)
* **Admin:** `admin@internflow.com` (Toàn quyền hệ thống)
* **Mentor:** `mentor@internflow.com` (Đã được cấu hình `MaxMentees = 5`)
* **Intern:** `intern@internflow.com` (Đã có hồ sơ sinh viên `SV2026001`)

### B. Dữ liệu Phục vụ Môi trường Dev (`DevSeeder.cs` - Fix GUID cố định)
* **Mentor Demo:** `mentor.demo@internflow.com` (GUID: `11111111-1111-1111-1111-111111111111`)
* **Intern Demo:** `intern.demo@internflow.com` (GUID: `22222222-2222-2222-2222-222222222222`)
* **Admin Demo:** `admin@internflow.com` (GUID: `aaaaaaaa-0000-0000-0000-000000000001`)
