using AutoMapper;
using InternFlow.Application.Features.Projects;

namespace InternFlow.Tests.Common.TestHelpers;

public static class TestMapperFactory
{
    private static readonly Lazy<IMapper> _lazyMapper = new(() =>
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProjectMappingProfile>();

        });

        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    });
    public static IMapper Instance => _lazyMapper.Value;
}
