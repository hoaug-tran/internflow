using AutoMapper;
using InternFlow.Domain.Common;

namespace InternFlow.Application.Common.Extensions;

public static class MappingExpressionExtensions
{
    public static IMappingExpression<TSource, TDestination> IgnoreAuditAndNavigation<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> expression)
        where TDestination : AuditableEntity
    {
        return expression
            .ForMember("Id", o => o.Ignore())
            .ForMember("CreatedAt", o => o.Ignore())
            .ForMember("UpdatedAt", o => o.Ignore())
            .ForMember("RowVersion", o => o.Ignore());
    }
}
