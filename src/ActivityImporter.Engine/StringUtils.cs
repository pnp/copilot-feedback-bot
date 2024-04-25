using System.Web;

namespace ActivityImporter.Engine;

public class StringUtils
{
    public static string GetOnlineMeetingId(string copilotDocContextId, string userGuid)
    {
        var meetingIdFragment = StringUtils.GetMeetingIdFragmentFromMeetingThreadUrl(copilotDocContextId);
        if (meetingIdFragment == null)
        {
            throw new Exception($"Could not parse meeting id from url {copilotDocContextId}");
        }

        // Get/create meeting in DB
        var meetingId = $"{userGuid}_{meetingIdFragment}";
        return meetingId;
    }

    public static string? GetDriveItemId(string copilotDocContextId)
    {
        var uri = new Uri(copilotDocContextId);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var sourcedoc = query["sourcedoc"];
        if (string.IsNullOrEmpty(sourcedoc))
        {
            return null;
        }
        return sourcedoc.Replace("{", "").Replace("}", "");
    }
    public static string? GetSiteUrl(string copilotDocContextId)
    {
        if (!Uri.IsWellFormedUriString(copilotDocContextId, UriKind.Absolute))
        {
            return null;
        }
        var uri = new Uri(copilotDocContextId);

        const string SEP = "/";

        // Remove SharePoint app pages (layouts)
        var absoluteUriMinusQueryAndAppPages = uri.AbsolutePath.Replace("/_layouts/15", "");

        var urlSegments = absoluteUriMinusQueryAndAppPages.Split(SEP, StringSplitOptions.RemoveEmptyEntries).ToList();

        var siteRelativeUrl = string.Empty;
        if (urlSegments.Count < 2)
        {
            if (urlSegments.Count == 0)
            {
                // URL is literally just a root SC URL like https://test.sharepoint.com
                return null;
            }
        }
        else
        {
            siteRelativeUrl = "/" + string.Join(SEP, urlSegments.Take(2).ToArray());
        }

        var siteUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{siteRelativeUrl}";
        return siteUrl;
    }
    public static bool IsMySiteUrl(string copilotDocContextId)
    {
        return copilotDocContextId.Contains("-my.sharepoint.com");
    }

    /// <summary>
    /// To form path required by https://learn.microsoft.com/en-us/graph/api/site-get
    /// From "https://test.sharepoint.com/sites/test" returns "test.sharepoint.com:/sites/test"
    /// Or, from root site "https://test.sharepoint.com" returns "root"
    /// Or null if none of the above
    /// </summary>
    public static string? GetHostAndSiteRelativeUrl(string siteRootUrl)
    {
        const string SEP = "/";
        var urlSegments = siteRootUrl.Split(SEP, StringSplitOptions.RemoveEmptyEntries);
        if (urlSegments.Length < 4)
        {
            if (urlSegments.Length == 2 && urlSegments[1].ToLower().Contains("sharepoint.com"))
                return "root"; // URL is literally just a root SC URL like https://test.sharepoint.com
            else return null;
        }

        var host = urlSegments[1];
        var siteRelativeUrl = string.Join(SEP, urlSegments.Skip(2).ToArray());

        return $"{host}:/{siteRelativeUrl}";
    }

    /// <summary>
    /// Example: https://microsoft.teams.com/threads/19:meeting_NDQ4MGRhYjgtMzc5MS00ZWMxLWJiZjEtOTIxZmM5Mzg3ZGFi@thread.v2 -> 
    ///          19:meeting_NDQ4MGRhYjgtMzc5MS00ZWMxLWJiZjEtOTIxZmM5Mzg3ZGFi@thread.v2
    /// </summary>
    public static string? GetMeetingIdFragmentFromMeetingThreadUrl(string copilotDocContextId)
    {
        const string START = "19:meeting_";
        var start = copilotDocContextId.IndexOf(START);
        if (start == -1)
        {
            return null;
        }
        var meetingId = copilotDocContextId.Substring(start, copilotDocContextId.Length - start);
        return meetingId;
    }
}
