using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ActivityImporter.Engine.ActivityAPI
{
    public class ActivitySubscriptionManager : AbstractActivityApiLoaderWithHttpClient, IActivitySubscriptionManager
    {
        private readonly string _tenantId;
        private readonly IEnumerable<string> _contentTypesToRead;

        public ActivitySubscriptionManager(string tenantId, ILogger telemetry, ConfidentialClientApplicationThrottledHttpClient httpClient, IEnumerable<string> contentTypesToRead)
            : base(telemetry, httpClient)
        {
            _tenantId = tenantId;
            _contentTypesToRead = contentTypesToRead;
        }

        /// <summary>
        /// Create any subscription that's configured but not active.
        /// </summary>
        public async Task CreateInactiveSubcriptions(List<string> active)
        {
            // Try and create it
            foreach (var configuredContentType in _contentTypesToRead)
            {
                if (!active.Contains(configuredContentType))
                {
                    try
                    {
                        Console.WriteLine("Creating subscription for content-type {0}. Need to wait a few seconds before it can be accessed...", configuredContentType);
                        var url = $"https://manage.office.com/api/v1.0/{_tenantId}/activity/feed/subscriptions/start?ContentType={configuredContentType}";

                        Console.WriteLine("+{0}", url);
                        var response = await _httpClient.PostAsync(url, null);

                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (HttpRequestException)
                        {
                            _telemetry.LogInformation("Can't create subscription. Check service-account permissions to Office 365 Activity API & that audit-log is turned on for tenant.");
                            _telemetry.LogInformation("https://docs.microsoft.com/en-gb/microsoft-365/compliance/turn-audit-log-search-on-or-off?view=o365-worldwide");
                            throw;
                        }

                        // Need to wait a few seconds before a new one can be accessed
                        Thread.Sleep(5000);
                        Console.WriteLine($"Subscription for '{configuredContentType}' has been created.");
                    }
                    catch (HttpRequestException ex)
                    {
                        // If we can't create it report the error
                        _telemetry.LogInformation($"Subscription for '{configuredContentType}' could not be found or created - {ex.Message}. Check the configuration file & app permissions in Azure AD.");
                        throw;
                    }
                }
            }
        }

        public async Task<List<string>> EnsureActiveSubscriptionContentTypesActive()
        {
            var active = await GetActiveSubscriptionContentTypes();
            var haveAllSubscriptionsActive = active.Count == _contentTypesToRead.Count();


            // If we don't have any subscriptions, create them
            if (!haveAllSubscriptionsActive)
            {
                await CreateInactiveSubcriptions(active);
                active = await GetActiveSubscriptionContentTypes();
            }

            return active;
        }

        public async Task<List<string>> GetActiveSubscriptionContentTypes()
        {
            List<string> validContentTypes = new List<string>();
            var allSubs = await GetActiveSubscriptions();

            foreach (var contentType in _contentTypesToRead)
            {
                // Try and find content type in all subs
                var sub = allSubs.Where(c => c.contentType == contentType).FirstOrDefault();

                if (sub != null)
                {
                    if (sub.status.ToLower() != "enabled")
                    {
                        _telemetry.LogInformation(string.Format("Subscription for '{0}' is already in place, but not enabled.", contentType));
                    }
                    else
                    {
                        _telemetry.LogInformation(string.Format("Subscription for '{0}' is already in place and enabled.", contentType));
                        validContentTypes.Add(contentType);
                    }
                }
            }

            return validContentTypes;
        }

        /// <summary>
        /// Fetch the list of subscriptions
        /// </summary>
        public async Task<ApiSubscription[]> GetActiveSubscriptions()
        {
            var url = $"https://manage.office.com/api/v1.0/{_tenantId}/activity/feed/subscriptions/list";
            var response = await _httpClient.GetAsync(url);
            _telemetry.LogInformation("Reading existing Office 365 Activity API subscriptions...");

            string responseBody = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<ApiSubscription[]>(responseBody) ?? new List<ApiSubscription>().ToArray();

        }
    }
}
