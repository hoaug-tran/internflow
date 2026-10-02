# Pull Request

<!--
Template dành cho team intern.

Trước khi tạo PR:
- Chỉ đưa các thay đổi liên quan đến task hiện tại vào PR.
- Tự kiểm tra code và Git diff trước.
- Build và test phải pass.
- Không merge trực tiếp vào main.
-->

## 1. Thông tin chung

**Issue / Task liên quan:**  
Closes #

**Mục đích của PR:**

<!--
Mô tả ngắn gọn:
- Làm task gì?
- Sửa vấn đề gì?
- Thêm chức năng gì?
-->

<!-- Mô tả tại đây -->

---

## 2. Loại thay đổi

<!-- Chọn loại phù hợp nhất. -->

- [ ] `feat` — Thêm tính năng mới
- [ ] `fix` — Sửa lỗi
- [ ] `refactor` — Cấu trúc lại code, không thay đổi nghiệp vụ
- [ ] `perf` — Cải thiện hiệu năng
- [ ] `test` — Thêm / sửa test
- [ ] `docs` — Cập nhật tài liệu
- [ ] `chore` — Cấu hình, dependency, CI/CD hoặc công việc kỹ thuật khác

---

## 3. Phạm vi thay đổi

<!-- Chọn các layer / project có thay đổi. -->

- [ ] `InternFlow.Domain`
- [ ] `InternFlow.Application`
- [ ] `InternFlow.Infrastructure`
- [ ] `InternFlow.API`
- [ ] `InternFlow.Shared`

**Feature / Module bị ảnh hưởng:**

<!--
Ví dụ:
- Authentication
- Internship
- Student
- Company
-->

<!-- Mô tả tại đây -->

---

## 4. Database / EF Core Migration

**Có thay đổi database không?**

- [ ] Không
- [ ] Có

**Migration name:** `N/A`

Nếu có migration:

- [ ] Đã kiểm tra migration generated code
- [ ] Đã chạy `dotnet ef database update` thành công
- [ ] Database sau migration hoạt động bình thường

<!--
Nếu migration có DROP COLUMN, đổi datatype,
thay đổi constraint hoặc dữ liệu hiện tại thì ghi rõ bên dưới.
-->

**Ghi chú migration:**

<!-- N/A hoặc mô tả tại đây -->

---

## 5. Build & Code Quality

- [ ] Đã chạy `dotnet format`
- [ ] Đã chạy `dotnet build`
- [ ] Build không có error
- [ ] Không phát sinh warning mới bất thường
- [ ] Không còn unused `using`, code thừa hoặc commented code
- [ ] Không còn debug log / code test tạm
- [ ] Code tuân theo `.editorconfig`
- [ ] Naming class, method, variable rõ nghĩa

---

## 6. Test

- [ ] Đã chạy `dotnet test`
- [ ] Các test liên quan đều pass
- [ ] Đã thêm / cập nhật test nếu logic mới cần test
- [ ] Không có test bị skip hoặc comment out bất thường
- [ ] Không áp dụng

Nếu PR có API:

- [ ] Đã test bằng Swagger / Postman
- [ ] Status code trả về đúng
- [ ] Request / Response đúng
- [ ] Validation hoạt động đúng
- [ ] Authentication / Authorization hoạt động đúng nếu có
- [ ] Đã kiểm tra các trường hợp lỗi liên quan

### Kết quả test

<!-- Có thể paste log hoặc request/response cần thiết. -->

```text
N/A
```

---

## 7. Clean Architecture

- [ ] Code được đặt đúng layer
- [ ] `Domain` không phụ thuộc layer bên ngoài
- [ ] `Application` không phụ thuộc implementation của `Infrastructure`
- [ ] Business logic không đặt trực tiếp trong API / Endpoint
- [ ] Feature mới tuân theo cấu trúc CQRS hiện tại
- [ ] Không tạo abstraction / helper không cần thiết

---

## 8. Security

- [ ] Không hardcode password, token, secret hoặc connection string
- [ ] Không commit `.env` hoặc credential
- [ ] Input từ client được validate
- [ ] Authorization đã được kiểm tra nếu endpoint yêu cầu
- [ ] Không log dữ liệu nhạy cảm
- [ ] Không áp dụng

---

## 9. Self-review trước khi yêu cầu review

<!-- Dev tạo PR phải tự kiểm tra phần này trước. -->

- [ ] PR chỉ chứa code liên quan đến task hiện tại
- [ ] Không có file bị sửa ngoài ý muốn
- [ ] Không có duplicate code rõ ràng
- [ ] Không có magic value nên đưa vào constant / configuration
- [ ] Exception được xử lý phù hợp
- [ ] Không có `catch` rỗng
- [ ] Đã xem lại toàn bộ Git diff
- [ ] CI pipeline đã pass

---

## 10. Ghi chú cho Reviewer

**Phần cần review kỹ:**

<!--
Ví dụ:
- Logic validation
- Authorization
- Query EF Core
- Migration
- Business rule
-->

N/A

**Cách test nhanh:**

<!--
Nếu reviewer cần seed data, account test,
request mẫu hoặc flow cụ thể thì ghi ở đây.
-->

N/A