using Entities.DB.Entities.AuditLog;

namespace ActivityImporter.Engine.ActivityAPI.Models;

public class CopilotAuditLogContent : AbstractAuditLogContent
{
    public CopilotEventData CopilotEventData { get; set; } = null!;
    public override async Task<bool> ProcessExtendedProperties(ActivityLogSaveSession sessionContext, CommonAuditEvent relatedAuditEvent)
    {
        await sessionContext.CopilotEventResolver.SaveSingleCopilotEventToSql(CopilotEventData, relatedAuditEvent);
        return true;
    }
}

public class CopilotEventData
{
    public AccessedResource[] AccessedResources { get; set; } = [];
    public string AppHost { get; set; } = null!;
    public List<Context> Contexts { get; set; } = [];
    public object[] MessageIds { get; set; } = [];
    public string ThreadId { get; set; } = null!;
}

public class Context
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
}


public class AccessedResource
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SensitivityLabelId { get; set; } = null!;
    public string Type { get; set; } = null!;
}
