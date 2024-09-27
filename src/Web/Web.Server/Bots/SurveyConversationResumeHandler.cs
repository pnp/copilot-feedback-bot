using Common.Engine.Notifications;
using Common.Engine.Surveys;
using Entities.DB.Entities.AuditLog;
using Microsoft.Bot.Schema;
using Web.Bots.Cards;

namespace Web.Bots;

public class SurveyConversationResumeHandler(IServiceProvider services) : IConversationResumeHandler
{
    public async Task<(BaseCopilotSpecificEvent?, Attachment)> GetProactiveConversationResumeConversationCard(string chatUserUpn)
    {
        using (var scope = services.CreateScope())
        {
            var surveyManager = scope.ServiceProvider.GetRequiredService<SurveyManager>();

            SurveyPendingActivities userPendingEvents;
            Entities.DB.Entities.User? dbUser = null;
            try
            {
                dbUser = await surveyManager.Loader.GetUser(chatUserUpn);
            }
            catch (ArgumentOutOfRangeException)
            {
                // User doesn't exist in our database, so assume they have no pending events
            }

            if (dbUser != null)
            {
                userPendingEvents = await surveyManager.FindNewSurveyEvents(dbUser);
            }
            else
            {
                userPendingEvents = new SurveyPendingActivities();
            }

            // Send survey card
            Attachment? surveyCard = null;

            // Are there any pending events to be surveyed?
            if (userPendingEvents.IsEmpty)
            {
                // Send general survey card for no specific event
                return (null, new SurveyNotForSpecificAction().GetCardAttachment());
            }
            else
            {
                // Send feedback card for specific action
                var nextCopilotEvent = userPendingEvents.GetNext() ?? throw new ArgumentOutOfRangeException("Unexpected null next event");

                // Register that we're sending a survey for this event so we don't repeatedly ask for the same event
                await surveyManager.Loader.LogSurveyRequested(nextCopilotEvent.RelatedChat.AuditEvent);

                // Figure out what kind of event it is & what card to send
                if (nextCopilotEvent is CopilotEventMetadataFile)
                {
                    surveyCard = new CopilotFileActionSurveyCard((CopilotEventMetadataFile)nextCopilotEvent).GetCardAttachment();
                }
                else if (nextCopilotEvent is CopilotEventMetadataMeeting)
                {
                    surveyCard = new CopilotTeamsActionSurveyCard((CopilotEventMetadataMeeting)nextCopilotEvent).GetCardAttachment();
                }
                else
                {
                    surveyCard = new SurveyNotForSpecificAction().GetCardAttachment();
                }
                return (nextCopilotEvent, surveyCard);
            }
        }
    }
}
