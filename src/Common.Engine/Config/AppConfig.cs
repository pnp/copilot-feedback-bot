using Common.DataUtils.Config;
using Microsoft.Extensions.Configuration;

namespace Common.Engine.Config;

public class AppConfig : PropertyBoundConfig
{
    public AppConfig() : base()
    {
        ConnectionStrings = new AppConnectionStrings();
        AuthConfig = new AzureADAuthConfig();
    }

    public AppConfig(IConfiguration config) : base(config)
    {
    }

    [ConfigSection()]
    public AzureADAuthConfig AuthConfig { get; set; } = null!;

    /// <summary>
    /// Hack for dev testing
    /// </summary>
    [ConfigValue(true)]
    public string TestUPN { get; set; } = null!;

    [ConfigValue(true)]
    public string WebAppURL { get; set; } = null!;

    [ConfigSection()]
    public AppConnectionStrings ConnectionStrings { get; set; } = null!;
}

public class AppConnectionStrings : PropertyBoundConfig
{
    public AppConnectionStrings() : base()
    {
    }

    public AppConnectionStrings(IConfigurationSection config) : base(config) { }

    [ConfigValue]
    public string SQL { get; set; } = null!;

    [ConfigValue]
    public string Redis { get; set; } = null!;

    public string ServiceBusRoot { get; set; } = null!;


    [ConfigValue]
    public string Storage { get; set; } = null!;
}
