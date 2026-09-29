# InternFlow API — chạy bằng Docker

## Yêu cầu
- Docker Desktop (hoặc Docker Engine + Compose plugin)

## Chạy từ máy sạch

1. Clone repo:
   ```bash
   git clone https://github.com/hoaug-tran/internflow.git
   cd internflow
   ```

2. Tạo file `.env` từ mẫu:
   ```bash
   cp .env.example .env
   ```
   Mở `.env` và đổi `SA_PASSWORD`, `JWT_SECRET` thành giá trị của bạn (không dùng giá trị mẫu khi deploy thật).

3. Build và chạy:
   ```bash
   docker compose up --build
   ```
   Lần đầu sẽ mất vài phút để tải image SQL Server/Redis và build API.

4. Kiểm tra các service đã sẵn sàng:
   ```bash
   docker compose ps
   ```
   Cả 3 service (`api`, `sqlserver`, `redis`) phải ở trạng thái `healthy` (SQL Server/Redis) hoặc `running` (api).

5. Áp migration (chạy 1 lần sau khi container đã lên):
   ```bash
   dotnet ef database update --project InternFlow.Infrastructure --startup-project InternFlow.API
   ```
   *(Cần cài `dotnet-ef` local: `dotnet tool install --global dotnet-ef` nếu chưa có. Lệnh này chạy trên máy host, kết nối tới SQL Server đang chạy trong container qua cổng 1433.)*

6. Truy cập Swagger: http://localhost:8080/swagger

## Dừng và dọn dẹp

```bash
docker compose down          # dừng container, giữ lại dữ liệu DB
docker compose down -v       # dừng và xóa luôn volume DB (reset sạch)
```

## Cấu trúc

| File | Vai trò |
|---|---|
| `Dockerfile` | Build multi-stage image cho API (SDK build → ASP.NET runtime) |
| `docker-compose.yml` | API + SQL Server + Redis, có healthcheck, mạng riêng `internflow-net` |
| `.env.example` | Mẫu biến môi trường, copy thành `.env` (không commit `.env`) |

## Ghi chú
- `.env` đã có trong `.gitignore`, không được commit.
- Đổi port trong `docker-compose.yml` nếu máy bạn đã dùng cổng 8080/1433/6379.
- Nếu `sqlserver` không lên `healthy`, xem log: `docker compose logs sqlserver`.
