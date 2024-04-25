using Common.DataUtils.Config;
using Common.Engine.Config;
using Microsoft.Extensions.Configuration;

namespace UnitTests;

public class TestsConfig : AppConfig
{
    public TestsConfig(IConfiguration config) : base(config)
    {
    }

    [ConfigValue]
    public string TestCopilotDocContextIdMySites { get; set; } = null!;

    [ConfigValue]
    public string TestCopilotDocContextIdSpSite { get; set; } = null!;

    [ConfigValue]
    public string TestCopilotEventUPN { get; set; } = null!;


    [ConfigValue]
    public string MySitesFileUrl { get; set; } = null!;
    [ConfigValue]
    public string MySitesFileName { get; set; } = null!;

    [ConfigValue]
    public string MySitesFileExtension { get; set; } = null!;



    [ConfigValue]
    public string TeamSiteFileUrl { get; set; } = null!;
    [ConfigValue]
    public string TeamSitesFileName { get; set; } = null!;

    [ConfigValue]
    public string TeamSiteFileExtension { get; set; } = null!;


    [ConfigValue(true)]
    public string TestCallThreadId { get; set; } = null!;
}
