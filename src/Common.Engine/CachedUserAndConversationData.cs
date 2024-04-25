using Azure;
using Azure.Data.Tables;

namespace Common.Engine;

/// <summary>
/// Table storage or memory cache for user
/// </summary>
public class CachedUserAndConversationData : ITableEntity
{
    public static string PartitionKeyVal => "Users";
    public string PartitionKey { get => PartitionKeyVal; set { return; } }

    /// <summary>
    /// Azure AD ID
    /// </summary>
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    /// <summary>
    /// Gets or sets service URL.
    /// </summary>
    public string ServiceUrl { get; set; } = null!;

    public string ConversationId { get; set; } = null!;
    public string? UserPrincipalName { get; set; } = null;
}
