# MÔ TẢ PROJECT INTERNFLOW

**Tên dự án:** InternFlow – Hệ thống quản lý thực tập và phân công công việc

**Loại dự án:** ASP.NET Core Web API – Backend

## 1. Tổng quan dự án

InternFlow là hệ thống Backend được xây dựng nhằm hỗ trợ quản lý toàn bộ quá trình thực tập của sinh viên hoặc thực tập sinh trong doanh nghiệp, từ việc tổ chức đợt thực tập, phân công người hướng dẫn, quản lý dự án và công việc đến theo dõi tiến độ, ghi nhận thời gian làm việc, đánh giá kết quả và lập báo cáo.

Thay vì quản lý thông tin thực tập, công việc và kết quả đánh giá bằng nhiều công cụ riêng lẻ, InternFlow tập trung các nghiệp vụ vào một hệ thống thống nhất, giúp người quản lý theo dõi tiến độ, người hướng dẫn kiểm soát công việc và thực tập sinh cập nhật quá trình làm việc của mình.

Dự án được phát triển theo mô hình **RESTful API**, sử dụng **ASP.NET Core 8**, **SQL Server** và **Redis**, đồng thời áp dụng các kiến thức về Clean Architecture, CQRS, MediatR, Authentication, Authorization, Entity Framework Core, Dapper, Unit Test, Integration Test, Docker và CI.

Đây là project tổng hợp kiến thức của toàn bộ lộ trình thực tập ASP.NET Core Backend, được phát triển theo nhóm trên cùng một repository thông qua Git, Pull Request và Code Review.

## 2. Đối tượng sử dụng và phân quyền

Hệ thống được thiết kế cho ba nhóm người dùng chính:

- **Admin:** Quản lý đợt thực tập, Mentor, Intern, phân công hướng dẫn, dự án, kỳ đánh giá và theo dõi dữ liệu tổng hợp của hệ thống.
- **Mentor:** Hướng dẫn thực tập sinh, quản lý và theo dõi công việc trong phạm vi được phân công, đánh giá kết quả thực tập và xem các báo cáo liên quan.
- **Intern:** Tham gia dự án, tiếp nhận công việc, cập nhật trạng thái, ghi nhận thời gian làm việc và xem kết quả đánh giá của bản thân.

Hệ thống kết hợp phân quyền theo vai trò (**Role-Based Authorization**) với kiểm tra phạm vi dữ liệu (**Resource/Scope Authorization**), nhằm giới hạn quyền truy cập theo mối quan hệ giữa người dùng, đợt thực tập, dự án và công việc.

## 3. Các phân hệ chức năng chính

### 3.1. Identity và Authentication

Phân hệ chịu trách nhiệm xác thực người dùng, quản lý phiên đăng nhập và cung cấp thông tin định danh cho các nghiệp vụ khác.

- Xây dựng cơ chế đăng nhập bằng tài khoản và mật khẩu, sử dụng BCrypt để băm và xác minh mật khẩu.
- Sử dụng JWT Access Token để xác thực các yêu cầu truy cập API.
- Triển khai Refresh Token Rotation, lưu mã băm của Refresh Token, quản lý vòng đời token và hỗ trợ thu hồi phiên.
- Xử lý việc sử dụng lại Refresh Token đã bị thay thế và giới hạn số lần đăng nhập sai.
- Cung cấp API lấy thông tin tài khoản hiện tại và tích hợp thông tin người dùng vào quá trình kiểm tra phân quyền.

### 3.2. Internship và Mentor Assignment

Phân hệ quản lý các đợt thực tập, danh sách người tham gia và quan hệ hướng dẫn giữa Mentor và Intern.

- Tạo đợt thực tập với thông tin thời gian, trạng thái và người tạo.
- Quản lý vòng đời đợt thực tập theo các trạng thái `Planned`, `Active` và `Closed`.
- Thêm Mentor và Intern vào từng đợt thực tập, kiểm tra vai trò và điều kiện tham gia.
- Phân công Mentor hướng dẫn Intern, kiểm tra giới hạn số lượng thực tập sinh mà Mentor được phụ trách.
- Kết thúc phân công, lưu lịch sử thay đổi người hướng dẫn và theo dõi trạng thái tham gia của Intern.
- Hỗ trợ tìm kiếm, lọc, phân trang và xem thông tin chi tiết của đợt thực tập.

### 3.3. Project Management

Phân hệ hỗ trợ tổ chức dự án thực tế trong từng đợt thực tập và quản lý thành viên tham gia.

- Tạo và cập nhật dự án, quản lý thông tin mô tả, thời gian và trạng thái dự án.
- Quản lý các trạng thái `Planned`, `Active`, `OnHold`, `Completed` và `Cancelled`.
- Thêm, xóa mềm và khôi phục thành viên dự án theo các ràng buộc nghiệp vụ.
- Kiểm tra quyền truy cập dự án theo vai trò và quan hệ thành viên.
- Hỗ trợ danh sách dự án có tìm kiếm, lọc, phân trang và sắp xếp.
- Cung cấp dữ liệu tổng hợp công việc và thành viên trong phạm vi dự án.

### 3.4. Task Management và Worklog

Đây là phân hệ theo dõi quá trình thực hiện công việc và thời gian làm việc của thực tập sinh.

- Tạo và giao Task cho Intern, thiết lập độ ưu tiên, thời hạn, mô tả và người phụ trách.
- Triển khai quy trình xử lý công việc theo State Machine: `Todo → InProgress → InReview → Done`.
- Kiểm soát quyền chuyển trạng thái; Intern không được tự phê duyệt công việc từ `InReview` sang `Done`.
- Hỗ trợ đưa công việc về trạng thái đang thực hiện để chỉnh sửa hoặc hủy công việc theo quyền.
- Cho phép Intern tạo, cập nhật và xóa Worklog để ghi nhận ngày làm việc, số giờ và nội dung thực hiện.
- Áp dụng các quy tắc kiểm tra quyền sở hữu, giới hạn giờ làm, trạng thái Task và tính hợp lệ của dữ liệu.
- Hỗ trợ truy vấn danh sách Task/Worklog, lọc theo điều kiện, tìm kiếm và phân trang.

### 3.5. Evaluation Management

Phân hệ đánh giá giúp Mentor ghi nhận và công bố kết quả thực tập dựa trên các tiêu chí cụ thể.

- Quản lý kỳ đánh giá với các trạng thái `Draft`, `Open` và `Closed`.
- Xây dựng các tiêu chí đánh giá cùng trọng số tương ứng.
- Tạo phiếu đánh giá cho Intern, nhập điểm và nhận xét theo từng tiêu chí.
- Kiểm tra điểm trong khoảng 0–10, bảo đảm tổng trọng số tiêu chí bằng 100% khi công bố.
- Tự động tính điểm tổng hợp theo công thức trung bình có trọng số.
- Quản lý trạng thái phiếu `Draft → Published`; phiếu đã công bố không được chỉnh sửa.
- Cho phép Intern xem kết quả đánh giá thuộc phạm vi của bản thân.

### 3.6. Reporting và thống kê

Phân hệ báo cáo tổng hợp dữ liệu từ Task, Worklog và quan hệ Mentor–Intern, hỗ trợ theo dõi tình hình thực tập.

- **Task Progress Report:** Thống kê số công việc theo trạng thái, công việc quá hạn, tỷ lệ hoàn thành và tổng thời gian làm việc của từng Intern.
- **Intern Workload Report:** Tổng hợp số giờ làm việc, số Worklog, số ngày làm việc, số Task và thời gian làm việc trung bình.
- **Mentor Workload Report:** Thống kê số lượng Intern đang phụ trách và tổng khối lượng làm việc liên quan.
- Hỗ trợ lọc dữ liệu theo dự án và khoảng thời gian phù hợp với từng báo cáo.
- Sử dụng Dapper gọi SQL Server Stored Procedure để thực hiện các truy vấn báo cáo.
- Áp dụng giới hạn dữ liệu theo người yêu cầu: Admin có phạm vi tổng hợp rộng hơn, Mentor chỉ được xem dữ liệu thuộc phạm vi phụ trách.

## 4. Kiến trúc và công nghệ sử dụng

Hệ thống được tổ chức theo **Clean Architecture**, tách biệt nghiệp vụ, xử lý ứng dụng, truy xuất dữ liệu và giao tiếp HTTP.

| Thành phần | Vai trò |
|---|---|
| **InternFlow.API** | Controllers, REST API, Swagger, Middleware, JWT Authentication và cấu hình ứng dụng |
| **InternFlow.Application** | CQRS Commands/Queries, Handlers, DTOs, FluentValidation, MediatR Pipeline và các interface |
| **InternFlow.Domain** | Entities, trạng thái, quy tắc nghiệp vụ và các thành phần độc lập với hạ tầng |
| **InternFlow.Infrastructure** | EF Core, SQL Server, Migrations, Repositories, Dapper, Redis và các service triển khai |
| **InternFlow.Shared** | ApiResponse, Pagination, Exceptions và các thành phần dùng chung |
| **InternFlow.Tests** | Unit Test và Integration Test cho các nghiệp vụ, phân quyền và luồng xử lý |

Các công nghệ và kỹ thuật trọng tâm:

- **ASP.NET Core 8, C#:** Xây dựng các RESTful API và xử lý nghiệp vụ bất đồng bộ.
- **Clean Architecture, CQRS, MediatR:** Phân tách trách nhiệm, Commands/Queries và tổ chức xử lý theo Feature.
- **EF Core, SQL Server:** Quản lý dữ liệu quan hệ, Migration, Constraint và Transaction.
- **Dapper, Stored Procedure:** Thực hiện các truy vấn báo cáo và thống kê.
- **JWT, BCrypt:** Xác thực, bảo vệ mật khẩu và kiểm soát quyền truy cập.
- **FluentValidation, AutoMapper:** Kiểm tra dữ liệu đầu vào và ánh xạ giữa Entity/DTO.
- **Redis Cache:** Lưu dữ liệu truy vấn thường xuyên, cấu hình TTL và Cache Invalidation.
- **Serilog, Trace ID, Exception Middleware:** Ghi log, theo dõi request và xử lý lỗi tập trung.
- **Swagger/OpenAPI:** Tài liệu hóa API và hỗ trợ kiểm thử thủ công.
- **xUnit, Moq, Testcontainers:** Xây dựng Unit Test và Integration Test.
- **Docker, Docker Compose, GitHub Actions:** Đóng gói ứng dụng, cấu hình môi trường và tự động hóa kiểm tra mã nguồn.

## 5. Quy trình nghiệp vụ tổng thể

Một luồng hoạt động điển hình của hệ thống:

1. Admin tạo đợt thực tập, bổ sung Mentor và Intern, sau đó phân công Mentor phụ trách từng Intern.
2. Admin hoặc Mentor có quyền tạo dự án thuộc đợt thực tập và quản lý thành viên tham gia.
3. Người phụ trách dự án tạo Task, giao việc cho Intern và thiết lập thời hạn thực hiện.
4. Intern thực hiện công việc, cập nhật trạng thái Task và ghi nhận thời gian làm việc thông qua Worklog.
5. Mentor kiểm tra, phản hồi và phê duyệt công việc theo quy trình chuyển trạng thái đã được định nghĩa.
6. Khi đến kỳ đánh giá, Mentor chấm điểm Intern theo bộ tiêu chí, tính điểm tổng hợp và công bố kết quả.
7. Admin và Mentor sử dụng các báo cáo để theo dõi tiến độ, khối lượng công việc và kết quả thực tập.

Quy trình này liên kết các phân hệ thành một hệ thống nghiệp vụ thống nhất, thay vì chỉ triển khai các API CRUD độc lập.

## 6. Kiểm thử, quản lý mã nguồn và triển khai

Dự án được phát triển chung trên GitHub, có sử dụng branch, Pull Request và Code Review để tích hợp công việc giữa các thành viên.

Hệ thống đã triển khai Unit Test cho các quy tắc nghiệp vụ như xác thực, Refresh Token, phân công Mentor, quản lý dự án, Task Workflow, Worklog và tính điểm đánh giá. Ngoài ra, repository còn có Integration Test cho một số luồng Project, Task, Authorization và Cache Invalidation.

GitHub Actions được cấu hình tự động kiểm tra định dạng mã nguồn, Restore, Build và chạy Test khi có Push hoặc Pull Request vào nhánh `main`. Lần chạy CI thành công gần nhất được kiểm tra ghi nhận **244 bài test thành công, không có bài test thất bại**.

Về triển khai, Docker Compose được cấu hình để khởi chạy ba thành phần chính gồm ASP.NET Core API, SQL Server 2022 và Redis. Hệ thống có Health Check, cấu hình qua biến môi trường và hỗ trợ Migration/Seed dữ liệu ở môi trường Development.

## 7. Kết quả thực hiện và phạm vi hoàn thiện

InternFlow đã xây dựng và tích hợp phần lớn các chức năng chính của một hệ thống quản lý thực tập, đồng thời ứng dụng các kiến thức trọng tâm trong toàn bộ lộ trình đào tạo ASP.NET Core Backend.

Project thể hiện việc chuyển từ những bài thực hành độc lập sang phát triển một hệ thống có nhiều phân hệ, quan hệ dữ liệu phức tạp, quy tắc nghiệp vụ, phân quyền, caching, báo cáo và quy trình phát triển nhóm.

Tuy nhiên, hệ thống vẫn cần tiếp tục hoàn thiện một số nội dung trước khi nghiệm thu cuối kỳ, đặc biệt là kiểm soát quyền truy cập khi sử dụng Cache, chuẩn hóa HTTP Status Code, hoàn thiện quản lý tài khoản Admin, kiểm thử các tình huống bảo mật và đồng thời, bổ sung Integration Test cho Evaluation/Reporting và hoàn thiện tài liệu triển khai, bàn giao.

**Tổng kết:** InternFlow là project Backend quản lý thực tập có phạm vi nghiệp vụ tương đối đầy đủ, kết hợp nhiều công nghệ và kỹ thuật trong phát triển phần mềm thực tế. Dự án đáp ứng mục tiêu tổng hợp kiến thức của lộ trình thực tập, đồng thời tạo cơ sở để đánh giá khả năng phân tích nghiệp vụ, thiết kế kiến trúc, xây dựng API, quản lý database, bảo mật, kiểm thử và phối hợp phát triển phần mềm theo nhóm.
