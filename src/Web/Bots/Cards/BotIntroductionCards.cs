namespace Web.Bots.Cards;

public abstract class BaseBotIntroductionCard : CommonCardWithStartOrStopSurveysButtonCard
{
    public BaseBotIntroductionCard(string botName, bool userHasSurveysStopped) : base(userHasSurveysStopped)
    {
        BotName = botName;
    }

    public string BotName { get; set; }


    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.BotFirstIntroduction);
        return json;
    }
}

public class BotFirstIntroduction : BaseBotIntroductionCard
{
    public BotFirstIntroduction(string botName, bool userHasSurveysStopped) : base(botName, userHasSurveysStopped)
    {
    }

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.BotFirstIntroduction);
        return json;
    }
}

public class BotResumeConversationIntroduction : BaseBotIntroductionCard
{
    public BotResumeConversationIntroduction(string botName, bool userHasSurveysStopped) : base(botName, userHasSurveysStopped)
    {
    }

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.BotResumeConversationIntro);
        return json;
    }
}
