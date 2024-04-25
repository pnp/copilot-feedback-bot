using ActivityImporter.Engine.ActivityAPI.Models;

namespace ActivityImporter.Engine.ActivityAPI;

/// <summary>
/// One or more audit log reports. Usually loaded from the API (in WebActivityReportSet)
/// </summary>
public abstract class ActivityReportSet : List<AbstractAuditLogContent>
{
    #region Constructors

    public ActivityReportSet() : this(0) { }
    public ActivityReportSet(int capacity) : base(capacity) { }

    public ActivityReportSet(IEnumerable<AbstractAuditLogContent> collection) : base(collection)
    {
    }
    #endregion

    #region Props

    /// <summary>
    /// From all the reports, the earliest date-time for created
    /// </summary>
    public DateTime OldestContent
    {
        get
        {
            return this.OrderByDescending(c => c.CreationTime).Last().CreationTime;
        }
    }

    /// <summary>
    /// From all the reports, the latest date-time for created
    /// </summary>
    public DateTime NewestContent
    {
        get
        {
            return this.OrderByDescending(c => c.CreationTime).First().CreationTime;
        }
    }

    #endregion

    /// <summary>
    /// Write all to SQL with a new data cache for the events only in this content-set
    /// </summary>
    public async Task<ImportStat> CommitAllToSQL(IActivityReportPersistenceManager persistenceManager)
    {
        return await persistenceManager.CommitAll(this);
    }
}

/// <summary>
/// List of content loaded from the Activity API specifically
/// </summary>
public class WebActivityReportSet : ActivityReportSet
{
    #region Constructors

    /// <summary>
    /// Constructor for JSon deserialisation 
    /// </summary>
    public WebActivityReportSet()
    {
    }
    public WebActivityReportSet(ActivityReportInfo metaData) : this()
    {
        OriginalMetadata = metaData;
    }
    public WebActivityReportSet(int capacity) : base(capacity) { }

    public WebActivityReportSet(IEnumerable<AbstractAuditLogContent> collection) : base(collection)
    {
    }

    #endregion

    /// <summary>
    /// Original metadata used to populate content-set for
    /// </summary>
    public ActivityReportInfo OriginalMetadata { get; set; } = null!;
}
