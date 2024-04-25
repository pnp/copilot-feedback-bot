using Common.Engine.Surveys;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public class TimerFunctions
{
    private readonly ILogger<TimerFunctions> _tracer;
    private readonly ILogger<SurveyManager> _loggerSM;
    private readonly ISurveyManagerDataLoader _surveyManagerDataLoader;
    private readonly ISurveyProcessor _surveyProcessor;

    const string CRON_TIME = "0 0 * * * *";       // Every hour for debugging


    public TimerFunctions(ILogger<TimerFunctions> tracer, ILogger<SurveyManager> loggerSM, ISurveyManagerDataLoader surveyManagerDataLoader, ISurveyProcessor surveyProcessor)
    {
        _tracer = tracer;
        _loggerSM = loggerSM;
        _surveyManagerDataLoader = surveyManagerDataLoader;
        _surveyProcessor = surveyProcessor;
    }

    /// <summary>
    /// Trigger notifications. Find people that've done copilot things, and prompt for survey
    /// </summary>
    [Function(nameof(TriggerNotifications))]
    public async Task TriggerNotifications([TimerTrigger(CRON_TIME)] TimerJobRefreshInfo timerInfo)
    {
        _tracer.LogInformation($"{nameof(TriggerNotifications)} function executed at: {DateTime.Now}");
        var sm = new SurveyManager(_surveyManagerDataLoader, _surveyProcessor, _loggerSM);
        await sm.FindAndProcessNewSurveyEventsAllUsers();
        _tracer.LogInformation($"Next timer schedule at: {timerInfo.ScheduleStatus.Next}");
    }
}

public class TimerJobRefreshInfo
{
    public TimerJobRefreshScheduleStatus ScheduleStatus { get; set; } = null!;
    public bool IsPastDue { get; set; }
}

public class TimerJobRefreshScheduleStatus
{
    public DateTime Last { get; set; }
    public DateTime Next { get; set; }
    public DateTime LastUpdated { get; set; }
}
