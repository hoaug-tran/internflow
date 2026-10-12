# InternFlow — Module Project / Task / WorkLog / Cache

**Người thực hiện:** Tú · **Cập nhật:** 2026-10-12  
**Stack:** ASP.NET Core 8 · MediatR (CQRS) · FluentValidation · EF Core 8 · SQL Server 2022 · Redis 7 · xUnit + Moq + FluentAssertions + Testcontainers · Docker

## 1. Tổng quan

Module lõi nghiệp vụ: quản lý dự án thực tập, giao việc, theo dõi tiến độ và nhật ký làm việc hàng ngày của intern.

| Vai trò | Trách nhiệm |
|---|---|
| **Admin** | Toàn quyền trên mọi dự án, task, worklog |
| **Mentor** | Quản lý dự án mình là Lead, giao việc, theo dõi tiến độ intern |
| **Intern** | Nhận task, cập nhật trạng thái, ghi worklog |

| Entity | Bảng | Quan hệ |
|---|---|---|
| `Project` | `Projects` | N—1 Internship, 1—N Task, 1—N ProjectMember |
| `ProjectMember` | `ProjectMembers` | N—1 Project, N—1 User (Lead/Member), xóa mềm bằng `RemovedAt` |
| `TaskItem` | `Tasks` | N—1 Project, N—1 User (Assignee/Creator) |
| `WorkLog` | `WorkLogs` | N—1 Task, N—1 User, xóa mềm bằng `DeletedAt` |

**Vòng đời:** Admin tạo Internship → tạo Project (creator là Lead) → thêm Member → Lead giao Task → Intern đổi trạng thái và ghi WorkLog → Mentor xem Task Summary → đánh giá (module Evaluation).

## 2. Phân quyền

🟢 toàn quyền · 🟡 giới hạn phạm vi · 🔴 từ chối

| Chức năng | Admin | Mentor | Intern |
|---|:---:|:---:|:---:|
| Tạo dự án | 🟢 | 🟢 | 🔴 |
| Xem dự án | 🟢 | 🟡 mình tạo/tham gia | 🟡 mình là member |
| Sửa dự án | 🟢 | 🟡 Lead | 🔴 |
| Thêm/xóa/khôi phục member | 🟢 | 🟡 Lead (chỉ intern mình phụ trách) | 🔴 |
| Tạo/sửa task | 🟢 | 🟡 Lead | 🔴 |
| Xem task | 🟢 | 🟡 trong dự án mình | 🟡 task được giao |
| Todo→InProgress, InProgress→InReview | 🟢 | 🟢 | 🟡 task của mình |
| InReview→Done / →InProgress, Cancelled | 🟢 | 🟢 | 🔴 |
| Tạo/sửa/xóa/khôi phục worklog | 🔴 | 🔴 | 🟡 chỉ của mình |
| Xem worklog | 🟢 | 🟡 intern mình phụ trách | 🟡 của mình |
| Xem summary / members (cache) | 🟢 | 🟡 dự án mình | 🟡 dự án mình |

**Kiểm tra 2 lớp:** Role (`EnsureAuthenticated`/`EnsureRole`) → Scope (`ProjectAccessExtensions`: `EnsureCanViewProjectAsync`, `CanViewProjectAsync`, `EnsureCanManageProjectAsync`, `CanManageProjectAsync`, `ApplyProjectVisibilityScope`).

| Tình huống | Status |
|---|---|
| Chưa đăng nhập | 401 |
| Sai role / không đủ quyền thao tác (biết tài nguyên tồn tại) | 403 |
| Không tồn tại / không có quyền xem (ẩn sự tồn tại) | 404 |
| Sai luồng trạng thái / vi phạm rule | 409 |
| Dữ liệu đầu vào sai | 400 |

## 3. Nghiệp vụ chính

### 3.1. State machine Task

5 trạng thái, 7 bước chuyển hợp lệ. Sai luồng → 409, đúng luồng sai role → 403.

| Từ → Sang | Admin | Mentor | Intern |
|---|:---:|:---:|:---:|
| Todo → InProgress | ✅ | ✅ | ✅ |
| InProgress → InReview | ✅ | ✅ | ✅ |
| InReview → Done | ✅ | ✅ | ❌ |
| InReview → InProgress (trả lại) | ✅ | ✅ | ❌ |
| Todo / InProgress / InReview → Cancelled | ✅ | ✅ | ❌ |
| Done, Cancelled → bất kỳ | ❌ | ❌ | ❌ |

### 3.2. Quy tắc Project / Member

- Tên project không trùng trong cùng đợt (tạo và sửa) → 409.
- Thêm intern phải thuộc đợt; Mentor chỉ thêm intern mình phụ trách, không thêm Mentor khác → 403.
- Xóa member là xóa mềm (`RemovedAt`); không xóa Lead cuối cùng, không xóa member còn task mở → 409.
- Tạo task: assignee phải là Intern, là member và thuộc đợt → 409.

### 3.3. Quy tắc WorkLog

- **Tạo** (Intern): task phải được giao cho mình (403); task không ở Todo/Cancelled; `WorkDate` ≤ hôm nay (tương lai → 400) và trong đợt; task Done chỉ ghi bù trong 3 ngày; `0 < Hours ≤ 12`, tổng giờ/ngày ≤ 12; không trùng (task, user, ngày); project không OnHold/Cancelled; intern active → các lỗi còn lại 409.
- **Sửa / Xóa** (owner): trong 7 ngày kể từ ngày tạo (quá hạn → 409); không phải owner hoặc đã xóa → 404. Xóa là xóa mềm (`DeletedAt`).

### 3.4. Khôi phục dữ liệu xóa mềm (mới)

| | `RestoreProjectMember` | `RestoreWorkLog` |
|---|---|---|
| Tác dụng | Đặt `RemovedAt = null` cho member đã bị xóa mềm | Đặt `DeletedAt = null` cho worklog đã bị xóa mềm |
| Ai được làm | Admin, Lead (như thêm/xóa member) | Owner (Intern) |
| Lỗi điển hình | Project/member không tồn tại → 404; member đang active → 409 | Worklog không tồn tại hoặc không phải của mình → 404; đang chưa bị xóa → 409; trùng worklog active cùng (task, user, ngày) → 409 |
| Cache | Invalidate `project:members:{id}` + `task:summary:project:{id}` | Không ảnh hưởng cache |

## 4. Endpoints

| Nhóm | Method | Endpoint | Quyền | Ghi chú |
|---|---|---|---|---|
| Project | POST | `/api/projects` | Admin, Mentor | Creator là Lead |
| | GET | `/api/projects` | Mọi role (scope) | Paging + filter |
| | GET / PUT | `/api/projects/{id}` | Member·Admin / Admin·Lead | 404 nếu không có quyền xem |
| | POST | `/api/projects/{id}/members` | Admin, Lead | |
| | DELETE | `/api/projects/{id}/members/{userId}` | Admin, Lead | Xóa mềm |
| | POST | *(route Restore — điền)* | Admin, Lead | **Mới**: khôi phục member |
| | GET | `/api/projects/{id}/members` | Member, Admin | **Cache 10'** |
| | GET | `/api/projects/{id}/tasks/summary` | Member, Admin | **Cache 5'** |
| Task | POST | `/api/projects/{id}/tasks` | Admin, Lead | |
| | GET | `/api/tasks`, `/api/tasks/{id}` | Mọi role (scope) | Tối đa 50/trang |
| | PATCH | `/api/tasks/{id}` | Admin, Lead | |
| | PATCH | `/api/tasks/{id}/status` | State machine | 409/403 |
| WorkLog | POST | `/api/worklogs` | Intern | |
| | PATCH / DELETE | `/api/worklogs/{id}` | Owner (7 ngày) | DELETE là xóa mềm |
| | POST | *(route Restore — điền)* | Owner | **Mới**: khôi phục worklog |
| | GET | `/api/worklogs`, `/api/worklogs/{id}` | Mọi role (scope) | Route suy từ handler, xác nhận lại |

## 5. Hệ thống cache

**Mục tiêu:** `GET /projects/{id}/tasks/summary` tốn ~230 ms (join + group by) → cache Redis còn ~12 ms (**~19x**), 0 query DB khi HIT.

```text
Controller → ValidationBehavior → CachingBehavior ─ HIT  → trả luôn (không vào handler)
                                                   └ MISS → Handler → DB → SET (TTL)
Write handler → SaveChanges → Publish(CacheInvalidationNotification(prefix))
              → CacheInvalidationHandler → RemoveByPrefixAsync (SCAN + DEL)
Redis chết    → catch + log warning → coi như MISS → DB → API vẫn 200
```

| Thành phần | Vai trò |
|---|---|
| `[Cacheable("prefix", ttlMinutes)]` | Query tự khai báo cache |
| `CachingBehavior` | Tự động Get/Set, bỏ qua command, try/catch cả GET và SET |
| `DefaultCacheKeyBuilder` | Sinh key (Guid → `:N`, nhiều property → hash SHA256 16 ký tự) |
| `CacheKeys` | Nguồn sự thật cho prefix invalidate, type-safe |
| `RedisCacheService` | `IDistributedCache` (1 key) + `IConnectionMultiplexer` (SCAN theo prefix; dùng SCAN thay KEYS để không block Redis) |

**Query cache:** `GetTaskSummaryQuery` (5'), `GetProjectMembersQuery` (10'). **Không cache:** projects, tasks, worklogs, current user (filter tự do, per-user, đọc-ghi liên tục).  
**Nguyên tắc:** cache khi đọc:ghi ≥ 10:1, kết quả không phụ thuộc user, filter giới hạn, có invalidate rõ ràng.

| Write handler | Invalidate |
|---|---|
| Create/Update/ChangeStatus Task | `task:summary:project:{projectId}` |
| Add/Remove/**Restore** ProjectMember | `project:members:{projectId}` + `task:summary:project:{projectId}` |

## 6. Test

- **Unit** (InMemory, không cần Docker): Domain (`TaskWorkflowTests` phủ toàn bộ ma trận), handler Project/Task/WorkLog, EF configuration (check constraint, index, concurrency token).
- **Integration** (Testcontainers SQL Server, `FakeCacheService`, cần Docker): health, authorization (401/404/400), project flow, task flow, cache invalidation. 23 test.
- Cần thêm test cho `RestoreProjectMember` và `RestoreWorkLog` (thành công, sai quyền, trạng thái không hợp lệ, trùng worklog active, invalidate cache).

```powershell
dotnet test .\InternFlow.Tests\InternFlow.Tests.csproj                                   # tất cả
dotnet test .\InternFlow.Tests\InternFlow.Tests.csproj --filter "FullyQualifiedName!~Integration"   # unit
dotnet test .\InternFlow.Tests\InternFlow.Tests.csproj --filter "FullyQualifiedName~Integration"    # integration
```

**Tổng số test:** `[điền số từ lần chạy dotnet test mới nhất]`

**Test thủ công cache:** `FLUSHDB` → gọi summary 2 lần (MISS → HIT) → đổi status task, kiểm tra `EXISTS` key = 0 và log `INVALIDATE` → `docker stop internflow-redis`, API vẫn 200 kèm log warning.

## 7. Điểm mạnh và hạn chế

**Điểm mạnh:** cache attribute-based (handler không biết cache); phân quyền 2 lớp với 404/403 rõ ràng; fallback DB khi Redis lỗi; invalidate và key không hard-code; state machine kiểm thử toàn diện; xóa mềm kèm khôi phục; test nhiều tầng.

**Hạn chế đã biết:**

| # | Hạn chế | Hướng xử lý |
|---|---|---|
| 1 | **Cache HIT bỏ qua handler → bỏ qua kiểm tra quyền** (key không gắn user) | Thêm `ProjectAuthorizationBehavior` trước `CachingBehavior` + integration test non-member. **Ưu tiên cao** |
| 2 | Integration test dùng `FakeCacheService`, chưa test SCAN thật | Redis Testcontainer |
| 3 | Chưa có unit test riêng cho `CachingBehavior` | Bổ sung |
| 4 | Chưa có endpoint DELETE task | Khi có, thêm invalidate |
| 5 | Invalidate lỗi chỉ log warning | Cache stale tối đa bằng TTL |
| 6 | Không đặt `InstanceName` cho Redis | Nếu đặt, phải sửa pattern SCAN |
