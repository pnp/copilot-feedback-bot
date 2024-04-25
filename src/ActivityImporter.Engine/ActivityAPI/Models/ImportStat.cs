namespace ActivityImporter.Engine.ActivityAPI.Models;

/// <summary>
/// Stats for work done on a batch
/// </summary>
public class ImportStat
{
    public int Imported { get; set; }
    public int ProcessedAlready { get; set; }
    public int URLsOutOfScope { get; set; }
    public int DownloadErrors { get; set; }
    public int Total { get; set; }

    public void AddStats(ImportStat statsToAdd)
    {
        if (statsToAdd == null)
        {
            throw new ArgumentNullException("statsToAdd");
        }
        ProcessedAlready += statsToAdd.ProcessedAlready;
        Imported += statsToAdd.Imported;
        URLsOutOfScope += statsToAdd.URLsOutOfScope;
        DownloadErrors += statsToAdd.DownloadErrors;
        Total += statsToAdd.Total;
    }

    public override string ToString()
    {
        return
            $"Imported successfully: {Imported.ToString("n0")}, " +
            $"already processed: {ProcessedAlready.ToString("n0")}, " +
            $"URLs out of scope (orgs table): {URLsOutOfScope.ToString("n0")}, " +
            $"errors: {DownloadErrors.ToString("n0")}, " +
            $"total: {Total.ToString("n0")}";
    }
}
