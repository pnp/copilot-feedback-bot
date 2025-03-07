﻿using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph;

public static class PageableGraphLoaderExtensions
{
    public static async Task<List<T>> LoadAllPagesWithThrottleRetries<T>(this ManualGraphCallClient client, string url, ILogger debugTracer)
    {
        var results = await LoadPageableGraphResponseAllWithOptionalDelta<T>(client, url, debugTracer, null);

        return results;
    }

    public static async Task<List<T>> LoadAllPagesPlusDeltaWithThrottleRetries<T>(this ManualGraphCallClient client, string url, ILogger debugTracer, Func<string, Task> deltaTokenFunc)
    {
        var results = await LoadPageableGraphResponseAllWithOptionalDelta<T>(client, url, debugTracer, deltaTokenFunc);

        return results;
    }

    static async Task<List<T>> LoadPageableGraphResponseAllWithOptionalDelta<T>(ManualGraphCallClient client, string url, ILogger debugTracer, Func<string, Task>? deltaTokenFunc)
    {
        var allResults = new List<T>();

        int pageCount = 1;

        // Loop until no pages left
        string? nextUrl = url;
        while (!string.IsNullOrEmpty(nextUrl))
        {
            var pageSuccess = false;
            PageableGraphResponseWithDelta<T>? queryResult = null;
            try
            {
                queryResult = await client.GetAsyncWithThrottleRetries<PageableGraphResponseWithDelta<T>>(nextUrl);
                pageSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                pageSuccess = false;
                debugTracer.LogError($"Got unexpected HTTP exception on page {pageCount}: {ex.Message}.");

                // Transient error?
                if (ex.Message != null && ex.Message.ToLower().Contains("gateway timeout"))
                {
                    debugTracer.LogWarning($"Got gateway timeout. Will retry page.");
                    await Task.Delay(1000);
                }
                else
                {
                    debugTracer.LogWarning($"Unexpected HTTP error. Will not retry page & returning results upto current page.");
                    nextUrl = null;
                }
            }

            if (pageSuccess)
            {
                // Another page?
                nextUrl = queryResult?.OdataNextLink;
                if (nextUrl != null)
                {
                    pageCount++;
                    debugTracer.LogInformation($"Loading {typeof(T).Name} results page #{pageCount}...");
                }
                else
                {
                    // Last page of results. Do we have a delta link?
                    if (!string.IsNullOrEmpty(queryResult?.DeltaLink) && deltaTokenFunc != null)
                    {
                        await deltaTokenFunc(queryResult.DeltaLink);
                    }
                }
                if (queryResult?.PageResults != null)
                {
                    allResults.AddRange(queryResult.PageResults);
                }
            }
        }

        return allResults;
    }
}
