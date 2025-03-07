using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;

public class GraphActivityLoader : IUserActivityLoader
{
    private readonly ManualGraphCallClient _client;
    private readonly ILogger _logger;

    public GraphActivityLoader(ManualGraphCallClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<TAbstractActivityRecord>> LoadReport<TAbstractActivityRecord>(DateTime dt, string reportGraphURL) where TAbstractActivityRecord : AbstractActivityRecord
    {
        // Load report
        var requestUrl = $"{reportGraphURL}(date={dt.ToString("yyyy-MM-dd")})?$format=application/json";
        _logger.LogDebug($"Loading usage from Graph {requestUrl}");

        var dayReports = await _client.LoadAllPagesWithThrottleRetries<TAbstractActivityRecord>(requestUrl, _logger);

        return dayReports;
    }
}

