using Common.DataUtils.Config;
using Microsoft.Extensions.Configuration;

namespace Common.Engine.Config;

public class TeamsAppConfig : AppConfig
{
    public TeamsAppConfig() : base()
    {
    }
    public TeamsAppConfig(IConfiguration config) : base(config)
    {
    }


    [ConfigSection()]
    public AzureADAuthConfig WebAuthConfig { get; set; } = null!;

    [ConfigValue(true)]
    public string? AppCatalogTeamAppId { get; set; } = null!;
}

/// <summary>
/// Configuration for the Teams bot
/// </summary>
public class BotConfig : TeamsAppConfig
{
    public BotConfig() : base()
    {
    }

    public BotConfig(IConfiguration config) : base(config)
    {
    }

    // Leaving these as default names for now so as to not mess with bot framework defaults
    [ConfigValue(backingPropertyName: "MicrosoftAppId")] public string BotAppId { get; set; } = null!;
    [ConfigValue(backingPropertyName: "MicrosoftAppPassword")] public string BotAppSecret { get; set; } = null!;
}
