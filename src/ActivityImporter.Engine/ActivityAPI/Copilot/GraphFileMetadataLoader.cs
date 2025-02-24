﻿using ActivityImporter.Engine.ActivityAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace ActivityImporter.Engine.ActivityAPI.Copilot;

/// <summary>
/// Populates file metadata from Graph API
/// </summary>
public class GraphFileMetadataLoader : ICopilotMetadataLoader
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly SiteGraphCache _siteGraphCache;
    private readonly UserGraphCache _userGraphCache;
    private readonly ILogger _logger;

    public GraphFileMetadataLoader(GraphServiceClient graphServiceClient, ILogger logger)
    {
        _graphServiceClient = graphServiceClient;
        _logger = logger;
        _siteGraphCache = new SiteGraphCache(graphServiceClient);
        _userGraphCache = new UserGraphCache(graphServiceClient);
    }

    public async Task<MeetingMetadata?> GetMeetingInfo(string meetingId, string userGuid)
    {
        // Requires OnlineMeetings.Read.All and https://learn.microsoft.com/en-us/graph/cloud-communication-online-meeting-application-access-policy#configure-application-access-policy

        try
        {
            var meeting = await _graphServiceClient.Users[userGuid].OnlineMeetings[meetingId].GetAsync();
            return new MeetingMetadata(meeting);
        }
        catch (ODataError ex)
        {
            LogGraphError(ex, $"getting meeting info for meetingId {meetingId}");
            return null;
        }
    }


    /// <summary>
    /// From a Copilot context ID, get the file metadata
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    // Example: https://m365cp123890-my.sharepoint.com/personal/sambetts_m365cp123890_onmicrosoft_com/_layouts/15/Doc.aspx?sourcedoc=%7B0D86F64F-8435-430C-8979-FF46C00F7ACB%7D&file=Presentation.pptx&action=edit&mobileredirect=true
    public async Task<SpoDocumentFileInfo?> GetSpoFileInfo(string copilotDocContextId, string eventUpn)
    {
        var siteUrl = StringUtils.GetSiteUrl(copilotDocContextId);
        if (siteUrl == null) return null;

        Drive? drive;
        if (StringUtils.IsMySiteUrl(siteUrl))
        {
            drive = await GetSpoInfoFromMySiteUrl(eventUpn);
        }
        else
        {
            drive = await GetSpoInfoFromSiteUrl(siteUrl);
        }
        if (drive == null)
        {
            return null;
        }

        // Get site ID from url
        // https://learn.microsoft.com/en-us/graph/api/drive-get?view=graph-rest-beta&tabs=http
        var spSiteId = drive.SharePointIds?.SiteId;
        if (string.IsNullOrEmpty(spSiteId))
        {
            throw new ArgumentOutOfRangeException("SharePointIds.SiteId");
        }
        var spListId = drive.SharePointIds?.ListId;
        if (string.IsNullOrEmpty(spListId))
        {
            throw new ArgumentOutOfRangeException("SharePointIds.ListId");
        }
        ListItem? item;
        var site = await _siteGraphCache.GetResourceOrNullIfNotExists(spSiteId);

        var driveItemId = StringUtils.GetDriveItemId(copilotDocContextId);
        if (driveItemId != null)
        {

            try
            {
                item = await _graphServiceClient.Sites[spSiteId].Lists[spListId].Items[driveItemId]
                    .GetAsync(op => op.QueryParameters.Expand = ["fields"]);
            }
            catch (ODataError ex) when (ex.ResponseStatusCode == 404)
            {
                LogGraphError(ex, $"getting item info for driveItemId {driveItemId}");
                return null;
            }

            return new SpoDocumentFileInfo(item, site);
        }
        else
        {
            // We might have a direct URL as the copilot context ID, so we need to search for the item in the list.
            // Example: https://contoso-my.sharepoint.com/personal/alex_contoso_onmicrosoft_com/Documents/MyDoc.docx
            try
            {
                // Currently we can't filter by webUrl, so we have to get all items and filter client side
                var listItems = await _graphServiceClient.Sites[spSiteId].Lists[spListId].Items
                    .GetAsync(op => op.QueryParameters.Select = ["id", "webUrl"]);
                if (listItems?.Value != null)
                {
                    foreach (var i in listItems.Value)
                    {
                        if (i.WebUrl == copilotDocContextId)
                        {
                            return new SpoDocumentFileInfo(i, site);
                        }
                    }
                }
            }
            catch (ODataError ex)
            {
                LogGraphError(ex, $"getting items info for list {spListId} on site {siteUrl}");
                return null;
            }

            _logger.LogWarning("No driveItemId found in copilotDocContextId {copilotDocContextId}", copilotDocContextId);
            return null;
        }
    }

    public async Task<string> GetUserIdFromUpn(string userPrincipalName)
    {
        var user = await _userGraphCache.GetResource(userPrincipalName);
        return user.Id ?? throw new Exception($"No user ID found on user in Graph by upn {userPrincipalName}");
    }

    static string[] DRIVE_FIELDS = ["SharePointIds,Id"];
    private async Task<Drive?> GetSpoInfoFromMySiteUrl(string eventUpn)
    {
        // Needs Files.Read.All
        try
        {
            return await _graphServiceClient.Users[eventUpn].Drive.GetAsync(o => o.QueryParameters.Select = DRIVE_FIELDS) ?? throw new ArgumentOutOfRangeException(eventUpn);
        }
        catch (ODataError ex)
        {
            LogGraphError(ex, $"getting drive info for user {eventUpn}");
            return null;
        }
    }

    private async Task<Drive?> GetSpoInfoFromSiteUrl(string siteUrl)
    {
        var siteAddress = StringUtils.GetHostAndSiteRelativeUrl(siteUrl);
        if (siteAddress == null)
        {
            // Possibly a Teams reference
            return null;
        }

        // Get drive ID from site ID
        Drive? siteDrive = null;
        try
        {
            siteDrive = await _graphServiceClient.Sites[siteAddress].Drive.GetAsync(o => o.QueryParameters.Select = DRIVE_FIELDS) ?? throw new ArgumentOutOfRangeException(siteAddress);
        }
        catch (ODataError)
        {
            // We can't get the drive via the site address, for some reason. Most of the time we can, but sometimes it doesn't work...
            // Load just the site and then try getting the drive using the loaded site ID
        }

        if (siteDrive == null)
        {
            Site? site = null;
            try
            {
                site = await _graphServiceClient.Sites[siteAddress].GetAsync() ?? throw new ArgumentOutOfRangeException(siteAddress);
            }
            catch (ODataError ex)
            {
                LogGraphError(ex, $"getting site info for site {siteUrl}");
                return null;
            }

            if (site != null)
            {
                try
                {
                    // Try one more time using site ID
                    siteDrive = await _graphServiceClient.Sites[site.Id].Drive.GetAsync(o => o.QueryParameters.Select = DRIVE_FIELDS) ?? throw new ArgumentOutOfRangeException(siteAddress);
                }
                catch (ODataError)
                {
                    // Ignore. Handle logging below
                }

                if (siteDrive == null)
                {
                    // Site exists but no drive for some reason
                    _logger.LogWarning($"No drive found for site ID {site.Id}");
                    return null;
                }
                else
                {
                    return siteDrive;
                }
            }
            else
            {
                // We can't find the site. Bug in the URL parsing?
                _logger.LogError("No site found for site {siteUrl}", siteUrl);
                return null;
            }
        }
        else
        {
            return siteDrive;
        }
    }
    void LogGraphError(ODataError error, string action)
    {
        _logger.LogWarning(error, $"Error '{error.Error?.Code}' {action}");
    }
}
