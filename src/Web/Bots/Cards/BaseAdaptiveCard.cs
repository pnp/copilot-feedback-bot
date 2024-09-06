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
    public string GetCardContentAndReplaceVars()
    {
        var content = ReplaceCardContentConstants(GetCardContent());
        return content;
    }

    protected abstract string GetCardContent();

    public virtual string ReplaceCardContentConstants(string raw)
    { 
        return raw.Replace(BotConstants.FIELD_NAME_BOT_NAME, BotConstants.BotName)
            .Replace(BotConstants.FIELD_NAME_SURVEY_STOP, BotConstants.SurveyAnswerStop)
            .Replace(BotConstants.FIELD_NAME_SURVEY_CONTINUE_SENDING, BotConstants.SurveyAnswerContinueSurveys);
    }

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
        dynamic cardJson = JsonConvert.DeserializeObject(ReplaceCardContentConstants(GetCardContentAndReplaceVars())) ?? new { };

        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = cardJson,
        };
    }
}
