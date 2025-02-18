namespace Common.Engine.Bot;

public static class BotConstants
{
    public static string FIELD_NAME_BOT_NAME => "${BotName}";
    public static string FIELD_NAME_MESSAGE => "${Message}";
    public static string FIELD_NAME_RESOURCE_NAME => "${ResourceName}";
    public static string FIELD_NAME_SURVEY_STOP => "${SurveyAnswerStop}";
    public static string BotName => "Copilot Feedback Bot";

    public static string BotFirstIntroduction => "Web.Server.Bots.Cards.Templates.BotFirstIntro.json";
    public static string BotDiagFinished => "Web.Server.Bots.Cards.Templates.BotDiagFinished.json";
    public static string BotReactionHappy => "Web.Server.Bots.Cards.Templates.BotReactionHappy.json";
    public static string BotReactionMeh => "Web.Server.Bots.Cards.Templates.BotReactionMeh.json";
    public static string BotResumeConversationIntro => "Web.Server.Bots.Cards.Templates.BotResumeConversationIntro.json";
    public static string CardFileNameCopilotTeamsActionSurvey => "Web.Server.Bots.Cards.Templates.SurveyCardTeamsAction.json";
    public static string CardFileNameCopilotFileActionSurvey => "Web.Server.Bots.Cards.Templates.SurveyCardFileAction.json";
    public static string CardFileNameSurveyNoActionCard => "Web.Server.Bots.Cards.Templates.SurveyCardNoAction.json";
    public static string SurveyOverrallSatisfactionCommonBody => "Web.Server.Bots.Cards.Templates.SurveyOverrallSatisfactionCommonBody.json";
    public static string SurveyCustomPageCommon => "Web.Server.Bots.Cards.Templates.SurveyCustomPageCommon.json";


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
