using Common.Engine.Config;
using Common.Engine.Notifications;

namespace Common.Engine.Surveys;

public class TeamsBotSendSurveyProcessor : ISurveyProcessor
{
    private readonly IBotConvoResumeManager _botConvoResumeManager;
    private readonly AppConfig _botConfig;

    public TeamsBotSendSurveyProcessor(IBotConvoResumeManager botConvoResumeManager, AppConfig botConfig)
    {
        _botConvoResumeManager = botConvoResumeManager;
        _botConfig = botConfig;
    }

    public async Task ProcessSurveyRequest(SurveyPendingActivities activities)
    {
#if DEBUG
        // Process only the test debug user
        foreach (var item in activities.FileEvents.Where(e => e.RelatedChat.AuditEvent.User.UserPrincipalName == _botConfig.TestUPN))
        {
            await _botConvoResumeManager.ResumeConversation(item.RelatedChat.AuditEvent.User.UserPrincipalName);
        }
#else
        // Process all users
        foreach (var item in activities.FileEvents)
        {
            await _botConvoResumeManager.ResumeConversation(item.RelatedChat.AuditEvent.User.UserPrincipalName);
        }
#endif

    }
}
