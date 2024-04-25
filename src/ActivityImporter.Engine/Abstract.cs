using Common.DataUtils.Http;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine;

public abstract class AbstractApiLoader
{
    protected readonly ILogger _telemetry;

    protected AbstractApiLoader(ILogger telemetry)
    {
        _telemetry = telemetry;
    }
}

public abstract class AbstractActivityApiLoaderWithHttpClient : AbstractApiLoader
{
    protected ConfidentialClientApplicationThrottledHttpClient _httpClient;
    protected AbstractActivityApiLoaderWithHttpClient(ILogger telemetry, ConfidentialClientApplicationThrottledHttpClient httpClient) : base(telemetry)
    {
        _httpClient = httpClient;
    }
}
