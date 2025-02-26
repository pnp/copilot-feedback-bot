using Entities.DB.Redis;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.GraphUser;

public class GraphUserLoader
{
    private readonly string _tenantId;
    private readonly ManualGraphCallClient _httpClient;
    private readonly ILogger _telemetry;

    private readonly CacheConnectionManager _cacheConnectionManager;
    public GraphUserLoader(string redisConnectionString, string tenantId, ManualGraphCallClient httpClient, ILogger telemetry)
    {
        _tenantId = tenantId;
        _httpClient = httpClient;
        _telemetry = telemetry;
        _cacheConnectionManager = CacheConnectionManager.GetConnectionManager(redisConnectionString);
    }

    public async Task<List<GraphUser>> LoadAllActiveUsers()
    {
        // Cache delta using tenant ID
        var REDIS_USER_DELTA_KEY = GetRedisUserDeltaCacheKey(_tenantId);
        var usersQueryDelta = await _cacheConnectionManager.GetString(REDIS_USER_DELTA_KEY);

        var initialDeltaUrl = $"https://graph.microsoft.com:443/v1.0/users/delta" +
            "?$select=id,accountEnabled,officeLocation,usageLocation,jobTitle,department,mail,userPrincipalName,manager,companyName,postalCode,country,state" +
            "&$expand=manager";
        if (!string.IsNullOrEmpty(usersQueryDelta))
        {
            initialDeltaUrl += $"&$deltatoken={usersQueryDelta}";
        }

        var results = await _httpClient.LoadAllPagesPlusDeltaWithThrottleRetries<GraphUser>(initialDeltaUrl, _telemetry,
            async (deltaLink) =>
            {
                var thisPageDelta = Common.DataUtils.CommonStringUtils.ExtractCodeFromGraphUrl(deltaLink);
                if (string.IsNullOrEmpty(thisPageDelta))
                {
                    await _cacheConnectionManager.DeleteString(REDIS_USER_DELTA_KEY);
                }
                else
                    await _cacheConnectionManager.SetString(REDIS_USER_DELTA_KEY, thisPageDelta);
            });


        if (string.IsNullOrEmpty(usersQueryDelta))
        {
            _telemetry.LogInformation($"User import - read {results.Count.ToString("N0")} users (all) from Graph API");
        }
        else
        {
            _telemetry.LogInformation($"User import - read {results.Count.ToString("N0")} updated users from Graph API, using last delta.");
        }

        // Graph for some reason gives duplicates; filter that out
        var allGraphUsers = results.GroupBy(u => u.UserPrincipalName).Select(g => g.First()).ToList();
        var allActiveGraphUsers = allGraphUsers.Where(u => u.AccountEnabled.HasValue && u.AccountEnabled.Value).ToList();

        return allActiveGraphUsers;
    }

    #region Redis

    public async Task ClearUserQueryDeltaCode()
    {
        var REDIS_USER_DELTA_KEY = GetRedisUserDeltaCacheKey(_tenantId);
        await _cacheConnectionManager.DeleteString(REDIS_USER_DELTA_KEY);
        _telemetry.LogInformation("User import - cleared delta token from cache");
    }

    static string GetRedisUserDeltaCacheKey(string tenantId)
    {
        var REDIS_USER_DELTA_KEY = $"UserDeltaCode-{tenantId}";
        return REDIS_USER_DELTA_KEY;
    }
    #endregion

}
