namespace Web.Bots;

public static class BotConstants
{
    public static string FIELD_NAME_BOT_NAME => "${BotName}";
    public static string FIELD_NAME_MESSAGE => "${Message}";
    public static string FIELD_NAME_RESOURCE_NAME => "${ResourceName}";
    public static string FIELD_NAME_SURVEY_STOP => "${SurveyAnswerStop}";
    public static string FIELD_NAME_SURVEY_CONTINUE_SENDING => "${SurveyAnswerContinueSurveys}";
    public static string BotName => "Copilot Feedback Bot";

    public static string BotFirstIntroduction => "Web.Bots.Cards.Templates.BotFirstIntro.json";
    public static string StopBotheringMe => "Web.Bots.Cards.Templates.StopBotheringMe.json";
    public static string BotDiagFinished => "Web.Bots.Cards.Templates.BotDiagFinished.json";
    public static string BotReactionHappy => "Web.Bots.Cards.Templates.BotReactionHappy.json";
    public static string BotReactionMeh => "Web.Bots.Cards.Templates.BotReactionMeh.json";
    public static string BotResumeConversationIntro => "Web.Bots.Cards.Templates.BotResumeConversationIntro.json";
    public static string CardFileNameCopilotTeamsActionSurvey => "Web.Bots.Cards.Templates.SurveyCardTeamsAction.json";
    public static string CardFileNameCopilotFileActionSurvey => "Web.Bots.Cards.Templates.SurveyCardFileAction.json";
    public static string CardFileNameSurveyNoActionCard => "Web.Bots.Cards.Templates.SurveyCardNoAction.json";
    public static string SurveyFollowUpQuestions => "Web.Bots.Cards.Templates.SurveyFollowUpQuestions.json";


    public static string SurveyCommonBody => "Web.Bots.Cards.Templates.Components.SurveyCommonBody.json";
    public static string BotReactionCommon => "Web.Bots.Cards.Templates.Components.BotReactionCommon.json";

    public static string SurveyButtonStopBotheringMe => "Web.Bots.Cards.Templates.Components.StopBotheringMeButton.json";
    public static string SurveyButtonContinueSurveys => "Web.Bots.Cards.Templates.Components.ContinueSurveysButton.json";


    public static string SurveyAnswerRating1 => "Terrible";
    public static string SurveyAnswerRating2 => "Useless";
    public static string SurveyAnswerRating3 => "Ok";
    public static string SurveyAnswerRating4 => "Useful";
    public static string SurveyAnswerRating5 => "Brilliant";
    public static string SurveyAnswerStop => "Stop";
    public static string SurveyAnswerContinueSurveys => "SurveyAnswerContinueSurveys";

    /// <summary>
    /// default value for channel activity to send notifications.
    /// </summary>
    public static string TeamsBotFrameworkChannelId => "msteams";
}
