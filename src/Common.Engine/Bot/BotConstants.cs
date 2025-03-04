namespace Common.Engine.Bot;

public static class BotConstants
{
    public static string FIELD_NAME_BOT_NAME => "${BotName}";
    public static string FIELD_NAME_MESSAGE => "${Message}";
    public static string FIELD_NAME_RESOURCE_NAME => "${ResourceName}";
    public static string FIELD_NAME_SURVEY_STOP => "${SurveyAnswerStop}";
    public static string BotName => "Copilot Feedback Bot";

    public static string BotFirstIntroduction => "Common.Engine.Bot.Cards.Templates.BotFirstIntro.json";
    public static string BotDiagFinished => "Common.Engine.Bot.Cards.Templates.BotDiagFinished.json";
    public static string BotReactionHappy => "Common.Engine.Bot.Cards.Templates.BotReactionHappy.json";
    public static string BotReactionMeh => "Common.Engine.Bot.Cards.Templates.BotReactionMeh.json";
    public static string BotResumeConversationIntro => "Common.Engine.Bot.Cards.Templates.BotResumeConversationIntro.json";
    public static string CardFileNameCopilotTeamsActionSurvey => "Common.Engine.Bot.Cards.Templates.SurveyCardTeamsAction.json";
    public static string CardFileNameCopilotFileActionSurvey => "Common.Engine.Bot.Cards.Templates.SurveyCardFileAction.json";
    public static string CardFileNameSurveyNoActionCard => "Common.Engine.Bot.Cards.Templates.SurveyCardNoAction.json";
    public static string SurveyOverrallSatisfactionCommonBody => "Common.Engine.Bot.Cards.Templates.SurveyOverrallSatisfactionCommonBody.json";
    public static string SurveyCustomPageCommon => "Common.Engine.Bot.Cards.Templates.SurveyCustomPageCommon.json";


    public static string SurveyAnswerRating1 => "Terrible";
    public static string SurveyAnswerRating2 => "Useless";
    public static string SurveyAnswerRating3 => "Ok";
    public static string SurveyAnswerRating4 => "Useful";
    public static string SurveyAnswerRating5 => "Brilliant";
    public static string SurveyAnswerStop => "Stop";

    /// <summary>
    /// default value for channel activity to send notifications.
    /// </summary>
    public static string TeamsBotFrameworkChannelId => "msteams";
}
