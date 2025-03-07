using System.Globalization;

namespace Common.DataUtils;

public static class CommonStringUtils
{
    /// <summary>
    /// https://contoso.sharepoint.com/sites/site/Shared Documents/
    /// to 
    /// https://contoso.sharepoint.com/sites/site/Shared%20Documents/
    /// Because Uri.IsWellFormedUriString doesn't recognise the 1st example as valid
    /// </summary>
    public static string ConvertSharePointUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        return url.Replace(" ", "%20");
    }

    /// <summary>
    /// Find the delta token in a Graph request URL
    /// </summary>
    public static string? ExtractCodeFromGraphUrl(string graphUrl)
    {
        // testUrl = https://graph.microsoft.com/v1.0/users/microsoft.graph.delta()?$deltatoken=xxxxxxxxxxxxxxxxxxxxxxxx

        const string TOKEN_START = "$deltatoken=";
        var tokenEqualStart = graphUrl.IndexOf(TOKEN_START);
        var tokenStart = tokenEqualStart + TOKEN_START.Length;
        if (tokenEqualStart > -1)
        {
            var token = graphUrl.Substring(tokenStart, graphUrl.Length - tokenStart);
            return token;
        }

        return null;
    }

    public static bool IsEmail(string input)
    {
        try
        {
            var m = new System.Net.Mail.MailAddress(input);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }

    }

    public static string ToGraphDateString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd");
    }

    public static DateTime? FromGraphDateString(string s)
    {
        DateTime dt;
        if (DateTime.TryParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        {
            return dt;
        }
        else
        {
            return null;
        }
    }

    public static string EnsureMaxLength(string? potentiallyLongString, int maxLength)
    {
        if (string.IsNullOrEmpty(potentiallyLongString))
        {
            return string.Empty;
        }
        const string END = "...";

        if (maxLength < END.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength));
        }

        if (potentiallyLongString.Length > maxLength)
        {
            return potentiallyLongString.Substring(0, maxLength - END.Length) + END;
        }
        else
        {
            return potentiallyLongString;
        }
    }
}
