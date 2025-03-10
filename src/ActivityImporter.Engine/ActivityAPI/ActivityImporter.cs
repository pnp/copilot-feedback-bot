﻿using ActivityImporter.Engine.ActivityAPI.Loaders;
using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.ActivityAPI;

/// <summary>
/// Audit log processor. Uses abstract loader implementations
/// </summary>
public abstract class ActivityImporter<SUMMARYTYPE> : AbstractApiLoader where SUMMARYTYPE : BaseActivityReportInfo
{
    private readonly int _maxSavesPerBatch;
    private int _reportSummariesTotal = 0;
    private int _reportSummariesProcessed = 0;
    private int _lastReportedPercentDone = 0;

    public const int MAX_DAYS_TO_DOWNLOAD = 7;

    public ActivityImporter(ILogger telemetry, int maxSavesPerBatch) : base(telemetry)
    {
        _maxSavesPerBatch = maxSavesPerBatch;
    }

    public abstract IActivityReportLoader<SUMMARYTYPE> ReportLoader { get; }
    public abstract ContentMetaDataLoader<SUMMARYTYPE> ContentMetaDataLoader { get; }
    public abstract IActivitySubscriptionManager ActivitySubscriptionManager { get; }


    public async Task<ImportStat> LoadReportsAndSave(IActivityReportPersistenceManager activityReportPersistenceManager)
    {
        var timer = new JobTimer(_telemetry, "Audit events import");
        timer.Start();

        var active = await ActivitySubscriptionManager.EnsureActiveSubscriptionContentTypesActive();

        var allStats = new ImportStat();
        var allSummaries = await ContentMetaDataLoader.GetChangesSummary(MAX_DAYS_TO_DOWNLOAD, active);        // Hack

        // Remember total so we can report on progress when threads finish loading a chunk
        lock (this)
        {
            _reportSummariesTotal = allSummaries.Count;
            _lastReportedPercentDone = 0;
        }

        await LoadFullReportsFromActivityApi(allSummaries, ReportLoader, async (reportChunk) =>
        {
            var stats = await activityReportPersistenceManager.CommitAll(new WebActivityReportSet(reportChunk));
            lock (allStats)
            {
                allStats.AddStats(stats);
            }

        });
#if DEBUG
        Console.WriteLine($"DEBUG: Got {allStats.Total.ToString("N0")} reports from {allSummaries.Count.ToString("N0")} summary reports");
#endif

        timer.TrackFinishedEventAndStopTimer(AnalyticsEvent.FinishedSectionImport);

        return allStats;
    }

    /// <summary>
    /// Process a new chunk of report summaries
    /// </summary>
    public async Task LoadFullReportsFromActivityApi(List<SUMMARYTYPE> reportSummaries, IActivityReportLoader<SUMMARYTYPE> activityReportLoader, Func<List<AbstractAuditLogContent>, Task> newReportsLoadedCallback)
    {
        // Sanity
        if (reportSummaries.Count == 0)
        {
            return;
        }

        // Only generate saves in batches of MAX_REPORTS_PER_THREAD. Call
        var listBatchProcessor = new ListBatchProcessor<AbstractAuditLogContent>(_maxSavesPerBatch, async (newChunk) => await newReportsLoadedCallback(newChunk));

        // For each summary chunk, load full reports in parallel
        var loader = new ParallelListProcessor<SUMMARYTYPE>(1000);

        // Load in parallel & call parent func on listBatchProcessor to save
        await loader.ProcessListInParallel(reportSummaries.OrderByDescending(j => j.Created),
            async (threadListChunk, threadIndex) => await ProcessSummaryChunkAsync(threadListChunk, listBatchProcessor, activityReportLoader),
                threads => _telemetry.LogInformation($"Audit events import: full-loading activity reports from {reportSummaries.Count.ToString("n0")} links, across {threads.ToString("n0")} thread(s)..."));

        listBatchProcessor.Flush();
    }

    private async Task ProcessSummaryChunkAsync(List<SUMMARYTYPE> summariesToLoad, ListBatchProcessor<AbstractAuditLogContent> listBatchProcessor, IActivityReportLoader<SUMMARYTYPE> activityReportLoader)
    {
        foreach (var job in summariesToLoad)
        {
            var metaReports = await activityReportLoader.Load(job);

            listBatchProcessor.AddRange(metaReports);

            // Update reports done stats
            lock (this)
            {
                _reportSummariesProcessed++;

                if (_reportSummariesProcessed > 0)
                {
                    var percentDone = _reportSummariesProcessed / (float)_reportSummariesTotal * 100;
                    if (percentDone < 100 && percentDone > 0)
                    {
                        int pcDone = Convert.ToInt32(Math.Round(percentDone, 0));
                        if (_lastReportedPercentDone < pcDone)
                        {
                            _telemetry.LogInformation($"Audit events import: processed {pcDone.ToString("n0")}% activity report data...");
                            _lastReportedPercentDone = pcDone;
                        }
                    }
                }
            }
        }
    }
}
