using Entities.DB.Entities.AuditLog;

namespace Web.Bots.Dialogues;

internal class SurveyDialogueConvoState
{
    public InitialSurveyResponse? LastResponse { get; set; }
    public int? NextCopilotCustomPage { get; set; }
    public int? SurveyIdUpdatedOrCreated { get; set; }
    public BaseCopilotSpecificEvent? CopilotEventForSurveyResult { get; set; }
}
