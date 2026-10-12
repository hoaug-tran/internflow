using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;

namespace InternFlow.Tests.Common.TestHelpers;

public static class RealUnitOfWorkFactory
{
    public static IUnitOfWork Create(InternFlowDbContext db) => db;
}