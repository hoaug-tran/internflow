using InternFlow.Domain.Entities;
using InternFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(InternFlowDbContext context)
    {
        // 1. Kiểm tra nếu đã có dữ liệu User thì không seed lại
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // Băm mật khẩu chuẩn BCrypt cho mật khẩu demo: "Password123!"
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        // 2. Tạo Admin
        var adminId = Guid.NewGuid();
        var adminUser = new User
        {
            Id = adminId,
            Email = "admin@internflow.com",
            FullName = "Hệ Thống Admin",
            Role = UserRole.Admin,
            PasswordHash = defaultPasswordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Tạo Mentor
        var mentorId = Guid.NewGuid();
        var mentorUser = new User
        {
            Id = mentorId,
            Email = "mentor@internflow.com",
            FullName = "Nguyễn Văn Mentor",
            Role = UserRole.Mentor,
            PasswordHash = defaultPasswordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            MentorProfile = new MentorProfile
            {
                UserId = mentorId,
                UserRole = UserRole.Mentor,
                Department = "Phát triển phần mềm",
                JobTitle = "Senior Backend Engineer",
                Expertise = "C# .NET, SQL Server, Clean Architecture",
                Phone = "0987654321",
                MaxMentees = 5,
                CreatedAt = DateTime.UtcNow
            }
        };

        // 4. Tạo Intern
        var internId = Guid.NewGuid();
        var internUser = new User
        {
            Id = internId,
            Email = "intern@internflow.com",
            FullName = "Trần Thị Thực Tập Sinh",
            Role = UserRole.Intern,
            PasswordHash = defaultPasswordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            InternProfile = new InternProfile
            {
                UserId = internId,
                UserRole = UserRole.Intern,
                StudentCode = "SV2026001",
                University = "Đại học Bách Khoa",
                Major = "Công nghệ Thông tin",
                Phone = "0123456789",
                DateOfBirth = new DateOnly(2003, 5, 20),
                CvUrl = "https://internflow.com/cvs/tran-thi-intern.pdf",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(adminUser, mentorUser, internUser);

        // 5. Tạo 1 đợt thực tập mẫu (Active)
        var batchId = Guid.NewGuid();
        var internship = new Internship
        {
            Id = batchId,
            Name = "Đợt Thực Tập Mùa Thu 2026",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(75)),
            Status = BatchStatus.Active,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        await context.Internships.AddAsync(internship);

        // 6. Gán Mentor và Intern vào đợt thực tập
        var batchMentor = new BatchMentor
        {
            BatchId = batchId,
            MentorId = mentorId,
            MentorRole = UserRole.Mentor,
            AddedBy = adminId,
            JoinedAt = DateTime.UtcNow
        };

        var batchIntern = new BatchIntern
        {
            BatchId = batchId,
            InternId = internId,
            InternRole = UserRole.Intern,
            Status = BatchInternStatus.InProgress,
            AddedBy = adminId,
            JoinedAt = DateTime.UtcNow
        };

        await context.BatchMentors.AddAsync(batchMentor);
        await context.BatchInterns.AddAsync(batchIntern);

        // 7. Phân công Mentor hướng dẫn Intern
        var assignment = new MentorAssignment
        {
            BatchId = batchId,
            InternId = internId,
            MentorId = mentorId,
            AssignedBy = adminId,
            AssignedAt = DateTime.UtcNow,
            EndedAt = null
        };

        await context.MentorAssignments.AddAsync(assignment);

        // Lưu tất cả vào DB
        await context.SaveChangesAsync();
    }
}
