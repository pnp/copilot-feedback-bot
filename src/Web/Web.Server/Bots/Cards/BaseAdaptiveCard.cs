﻿using AdaptiveCards;
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
        return Utils.ReadResource(resourcePath);
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
