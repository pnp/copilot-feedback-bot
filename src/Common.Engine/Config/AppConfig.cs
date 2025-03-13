using Common.DataUtils.Config;
using Microsoft.Extensions.Configuration;

namespace Common.Engine.Config;

public class AppConfig : PropertyBoundConfig
{
    public AppConfig() : base()
    {
        ConnectionStrings = new AppConnectionStrings();
        ImportAuthConfig = new AzureADAuthConfig();
    }

    public AppConfig(IConfiguration config) : base(config)
    {
    }

    [ConfigSection()]
    public AzureADAuthConfig ImportAuthConfig { get; set; } = null!;


    [ConfigValue(true, "APPLICATIONINSIGHTS_CONNECTION_STRING")]
    public string? AppInsightsConnectionString { get; set; }

    /// <summary>
    /// Hack for dev testing
    /// </summary>
    [ConfigValue(true)]
    public string TestUPN { get; set; } = null!;


    [ConfigValue(true)]
    public bool DevMode { get; set; } = false;

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
