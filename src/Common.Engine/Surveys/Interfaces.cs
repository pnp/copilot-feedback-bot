using Entities.DB.Entities;
using Entities.DB.Entities.AuditLog;

namespace Common.Engine.Surveys;

public interface ISurveyManagerDataLoader
{
    Task<DateTime?> GetLastUserSurveyDate(User user);
    Task<List<BaseCopilotSpecificEvent>> GetUnsurveyedActivities(User user, DateTime? from);
    Task<User> GetUser(string upn);
    Task<List<User>> GetUsersWithActivity();

    Task<SurveyPage?> GetSurveyPage(int pageIndex);


    /// <summary>
    /// Log survey result for a user, but for no specific copilot event. Returns the ID of the survey response created
    /// </summary>
    Task<int> LogDisconnectedSurveyResult(int scoreGiven, string userUpn);
    Task<int> LogSurveyRequested(CommonAuditEvent @event);

    Task StopBotheringUser(string upn, DateTime until);

    /// <summary>
    /// First response to the survey. Returns the ID of the survey response created
    /// </summary>
    Task<int> UpdateSurveyResultWithInitialScore(CommonAuditEvent @event, int score);
}

public interface ISurveyProcessor
{
    Task ProcessSurveyRequest(SurveyPendingActivities activities);
}
