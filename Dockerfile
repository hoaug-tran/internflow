# InternFlow API - Dockerfile (multi-stage)
# Giả định cấu trúc: InternFlow.Domain/, InternFlow.Application/, InternFlow.Infrastructure/,
# InternFlow.API/, InternFlow.Shared/ đặt ở gốc repo, cạnh InternFlow.slnx.
# Nếu thực tế nằm trong src/, thêm "src/" vào mọi đường dẫn COPY bên dưới.

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj trước để tận dụng cache layer khi chỉ code thay đổi
COPY InternFlow.slnx ./
COPY InternFlow.Domain/*.csproj ./InternFlow.Domain/
COPY InternFlow.Application/*.csproj ./InternFlow.Application/
COPY InternFlow.Infrastructure/*.csproj ./InternFlow.Infrastructure/
COPY InternFlow.API/*.csproj ./InternFlow.API/
COPY InternFlow.Shared/*.csproj ./InternFlow.Shared/

RUN dotnet restore InternFlow.API/InternFlow.API.csproj

# Copy toàn bộ source và build
COPY . .
WORKDIR /src/InternFlow.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Chạy bằng user không phải root
RUN useradd -m appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InternFlow.API.dll"]