using Entities.DB.Entities.AuditLog;
using Entities.DB.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB;

public class FakeDataGen
{
    public static async Task GenerateFakeCopilotFor(string forUpn, DataContext context, ILogger logger)
    {
        var editDoc = context.CopilotActivities.FirstOrDefault(a => a.Name == DbInitialiser.ACTIVITY_NAME_EDIT_DOC)!;
        var getHighlights = context.CopilotActivities.FirstOrDefault(a => a.Name == DbInitialiser.ACTIVITY_NAME_GET_HIGHLIGHTS)!;

        var user = context.Users.FirstOrDefault(u => u.UserPrincipalName == forUpn);
        if (user == null)
        {
            logger.LogWarning($"Creating new user with UPN {forUpn}");
            user = new User { UserPrincipalName = forUpn };
        }
        context.Add(new CopilotEventMetadataMeeting
        {
            RelatedChat = new CopilotChat
            {
                AppHost = "DevBox",
                AuditEvent = new CommonAuditEvent
                {
                    User = user,
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.Now.AddDays(-1),
                    Operation = new EventOperation { Name = "Meeting operation " + DateTime.Now.Ticks }
                }
            },
            OnlineMeeting = new OnlineMeeting { Name = "Weekly Team Sync", MeetingId = "Join Link" }
        });

        var fileName = $"Test File {DateTime.Now.Ticks}.docx";
        context.Add(new CopilotEventMetadataFile
        {

            RelatedChat = new CopilotChat
            {
                AppHost = "DevBox",
                AuditEvent = new CommonAuditEvent
                {
                    User = user,
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.Now.AddDays(-2),
                    Operation = new EventOperation { Name = "File operation " + DateTime.Now.Ticks }
                }
            },
            FileName = new SPEventFileName { Name = fileName },
            FileExtension = await context.SharePointFileExtensions.SingleOrDefaultAsync(e => e.Name == "docx"),
            Url = new Entities.SP.Url { FullUrl = $"https://devbox.sharepoint.com/Docs/{fileName}" },
            Site = context.Sites.FirstOrDefault()!,
        });
    }


    public static async Task GenerateFakeActivityFor(string forUpn, DataContext context, ILogger logger)
    {

        var user = context.Users.FirstOrDefault(u => u.UserPrincipalName == forUpn);
        if (user == null)
        {
            logger.LogWarning($"Creating new user with UPN {forUpn}");
            user = new User { UserPrincipalName = forUpn };
        }
        context.SharePointEvents.Add(new SharePointEventMetadata
        {
            AuditEvent = new CommonAuditEvent
            {
                User = user,
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.Now.AddDays(-1),
                Operation = new EventOperation { Name = "Operation " + DateTime.Now.Ticks }
            },
            ItemType = new SPEventType { Name = "Test" + DateTime.Now.Ticks },
            FileName = new SPEventFileName { Name = "Test File " + DateTime.Now.Ticks },
            RelatedSiteId = context.Sites.FirstOrDefault()?.ID ?? 0,
            Url = new Entities.SP.Url { FullUrl = $"https://devbox.sharepoint.com/Docs/TestFile{DateTime.Now.Ticks}.docx" },
            Site = context.Sites.FirstOrDefault() ?? new Entities.SP.Site { UrlBase = $"https://devbox.sharepoint.com/" },
            FileExtension = await context.SharePointFileExtensions.FirstOrDefaultAsync() ?? new SPEventFileExtension { Name = "TestExtension" },
        });

        context.OutlookUsageActivityLogs.Add(new Entities.UsageReports.OutlookUsageActivityLog
        {
            User = user,
            Date = DateTime.Now,
            LastActivityDate = DateTime.Now,
            MeetingsCreated = 1,
            MeetingsInteracted = 1,
            ReadCount = 1,
            SendCount = 1,
        });

        context.AppPlatformUserUsageLog.Add(new Entities.UsageReports.AppPlatformUserActivityLog
        {
            User = user,
            LastActivityDate = DateTime.Now,
            Date = DateTime.Now,
            Excel = true,
            ExcelMobile= true,
            ExcelWindows = true,
            ExcelMac = true,
            ExcelWeb = true,
            Outlook = true,
            OutlookMac = true,
            OutlookWindows = true,
            OutlookWeb = true,
            PowerPoint = true,
            PowerPointMobile = true,
            PowerPointWindows = true,
            PowerPointMac = true,
            PowerPointWeb = true,
            Teams = true,
            TeamsMobile = true,
            TeamsWindows = true,
            TeamsMac = true,
            TeamsWeb = true,
            Word = true,
            WordMobile = true,
            WordWindows = true,
            WordMac = true,
            WordWeb = true,
            OneNote = true,
            OneNoteMobile = true,
            OneNoteWindows = true,
            OneNoteMac = true,
            OneNoteWeb = true,
            Mac = true,
            Mobile = true,
            Web = true,
            Windows = true,
            OutlookMobile = true,
        });

        context.SharePointUserActivityLogs.Add(new Entities.UsageReports.SharePointUserActivityLog {
            Date = DateTime.Now,
            User = user,
            LastActivityDate = DateTime.Now,

            SharedExternally = 1,
            SharedInternally = 1,
            Synced = 1,
            ViewedOrEdited = 1,
        });

        context.OneDriveUserActivityLogs.Add(new Entities.UsageReports.OneDriveUserActivityLog
        {
            Date = DateTime.Now,
            User = user,
            LastActivityDate = DateTime.Now,
            SharedExternally = 1,
            SharedInternally = 1,
            Synced = 1,
            ViewedOrEdited = 1,
        });

        context.YammerUserActivityLogs.Add(new Entities.UsageReports.YammerUserActivityLog
        {
            Date = DateTime.Now,
            User = user,
            LastActivityDate = DateTime.Now,
            LikedCount = 1,
            PostedCount = 1,
            ReadCount = 1,
        });

        context.YammerDeviceActivityLogs.Add(new Entities.UsageReports.YammerDeviceActivityLog
        {
            Date = DateTime.Now,
            User = user,
            LastActivityDate = DateTime.Now,
            UsedAndroidPhone = true,
            UsedIpad = true,
            UsedIphone = true,
            UsedOthers = true,
            UsedWindowsPhone = true,
            UsedWeb = true,
        });

        context.TeamUserActivityLogs.Add(new Entities.UsageReports.GlobalTeamsUserUsageLog
        {
            Date = DateTime.Now,
            User = user,
            LastActivityDate = DateTime.Now,
            AdHocMeetingsAttendedCount = 1,
            AdHocMeetingsOrganizedCount = 1,
            CallCount = 1,
            MeetingCount = 1,
            MeetingsAttendedCount = 1,
            MeetingsOrganizedCount = 1,
            PrivateChatMessageCount = 1,
            TeamChatMessageCount = 1,
            PostMessages = 1,   
            ReplyMessages = 1,
            ScheduledOneTimeMeetingsAttendedCount = 1,
            ScheduledOneTimeMeetingsOrganizedCount = 1,
            ScheduledRecurringMeetingsAttendedCount = 1,
            ScheduledRecurringMeetingsOrganizedCount = 1,
            AudioDurationSeconds = 1,
            VideoDurationSeconds = 1,
            ScreenShareDurationSeconds = 1,
            UrgentMessages = 1,
        });

        context.TeamsUserDeviceUsageLog.Add(new Entities.UsageReports.GlobalTeamsUserDeviceUsageLog
        {
            Date = DateTime.Now,
            User = user,
            LastActivityDate = DateTime.Now,
            UsedAndroidPhone = true,
            UsedChromeOS = true,
            UsedIOS = true,
            UsedLinux = true,
            UsedMac = true,
            UsedWindows = true,
            UsedWindowsPhone = true,
            UsedWeb = true,
        });
    }
}
