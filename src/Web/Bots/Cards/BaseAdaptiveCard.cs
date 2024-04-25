using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Reflection;

namespace Web.Bots.Cards;

/// <summary>
/// Base implementation for any of the adaptive cards sent
/// </summary>
public abstract class BaseAdaptiveCard
{

    public abstract string GetCardContent();

    protected string ReadResource(string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        var manifests = assembly.GetManifestResourceNames();


        using (var stream = assembly.GetManifestResourceStream(resourcePath))
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(resourcePath), $"No resource found by name '{resourcePath}'");
            }
    }
    public Attachment GetCardAttachment()
    {
        dynamic cardJson = JsonConvert.DeserializeObject(GetCardContent()) ?? new { };

        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = cardJson,
        };
    }
}
