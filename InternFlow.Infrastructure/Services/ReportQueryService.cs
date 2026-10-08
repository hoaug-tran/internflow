using System.Data;
using Dapper;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Reports.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace InternFlow.Infrastructure.Services;

public class ReportQueryService(IConfiguration configuration) : IReportQueryService
{
    public async Task<IReadOnlyList<TaskProgressRowDto>> GetTaskProgressAsync(
        Guid requesterId, bool isAdmin, Guid? projectId, CancellationToken ct = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu connection string DefaultConnection.");

        await using var connection = new SqlConnection(connectionString);

        // Tham số hóa hoàn toàn, không ghép chuỗi SQL
        var command = new CommandDefinition(
            "dbo.usp_GetTaskProgressReport",
            new { RequesterId = requesterId, IsAdmin = isAdmin, ProjectId = projectId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct);

        var rows = await connection.QueryAsync<TaskProgressRowDto>(command);
        return rows.AsList();
    }
}
