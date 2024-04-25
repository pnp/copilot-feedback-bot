
using Azure;
using Azure.Data.Tables;

namespace Common.Engine.Notifications;

public class PropertyBagEntry : ITableEntity
{
    public const string PARTITION_NAME = "Properties";

    public PropertyBagEntry()
    {
    }

    public PropertyBagEntry(string property, string value)
    {
        PartitionKey = PARTITION_NAME;

        // Key is encoded URL
        RowKey = property;
        Value = value;
    }

    public string Value { get; set; } = string.Empty;
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
