using Common.Engine.Surveys;
using Newtonsoft.Json.Linq;

namespace Web.Bots.Cards;

public class BotReactionNoMoreQuestionsCard : BaseAdaptiveCard
{
    public BotReactionNoMoreQuestionsCard()
    {
    }

    public override string GetCardContent()
    {

        return ReadResource(BotConstants.BotReactionNoMoreQuestions);
    }
}
