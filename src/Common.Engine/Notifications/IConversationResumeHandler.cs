using Entities.DB.Entities.AuditLog;
using Microsoft.Bot.Schema;

namespace Common.Engine.Notifications;

public interface IConversationResumeHandler
{
    Task<(BaseCopilotSpecificEvent?, Attachment)> GetProactiveConversationResumeConversationCard(string chatUserUpn);
}
