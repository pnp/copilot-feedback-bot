using Entities.DB;
using Entities.DB.Entities;
using Entities.DB.Entities.AuditLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.Engine.Surveys;

/// <summary>
/// SQL Server implementation of the survey manager data loader
/// </summary>
public class SqlSurveyManagerDataLoader(DataContext db, ILogger<SqlSurveyManagerDataLoader> logger) : ISurveyManagerDataLoader
{
    public async Task<User> GetUser(string upn)
    {
        return await db.Users.Where(u => u.UserPrincipalName == upn).FirstOrDefaultAsync() ?? throw new ArgumentOutOfRangeException(nameof(upn));
    }

    public async Task<DateTime?> GetLastUserSurveyDate(User user)
    {
        var latestUserRespondedSurvey = await db.SurveyResponses
            .Where(e => e.User == user)
            .OrderBy(e => e.Responded).Take(1)
            .FirstOrDefaultAsync();

        if (latestUserRespondedSurvey != null)
        {
            return latestUserRespondedSurvey.Responded;
        }
        return null;
    }

    public async Task<List<BaseCopilotSpecificEvent>> GetUnsurveyedActivities(User user, DateTime? from)
    {
        var useRespondedEvents = await db.SurveyResponses
            .Include(e => e.RelatedEvent)
            .Where(e => e.RelatedEvent != null && e.RelatedEvent.User == user && (!from.HasValue || e.Requested > from))
            .Select(e => e.RelatedEvent)
            .ToListAsync();

        var fileEvents = await db.CopilotEventMetadataFiles
            .Include(e => e.RelatedChat)
                .ThenInclude(BaseCopilotSpecificEvent => BaseCopilotSpecificEvent.AuditEvent)
                    .ThenInclude(e => e.Operation)
            .Include(e => e.FileName)
            .Where(e => !useRespondedEvents.Contains(e.RelatedChat.AuditEvent) && e.RelatedChat.AuditEvent.User == user && (!from.HasValue || e.RelatedChat.AuditEvent.TimeStamp > from)).ToListAsync();

        var meetingEvents = await db.CopilotEventMetadataMeetings
            .Include(e => e.RelatedChat)
                .ThenInclude(BaseCopilotSpecificEvent => BaseCopilotSpecificEvent.AuditEvent)
                    .ThenInclude(e => e.Operation)
            .Include(e => e.OnlineMeeting)
            .Where(e => !useRespondedEvents.Contains(e.RelatedChat.AuditEvent) && e.RelatedChat.AuditEvent.User == user && (!from.HasValue || e.RelatedChat.AuditEvent.TimeStamp > from)).ToListAsync();

        return fileEvents.Cast<BaseCopilotSpecificEvent>().Concat(meetingEvents).ToList();
    }

    public async Task<int> LogSurveyRequested(CommonAuditEvent @event)
    {
        var survey = new UserSurveyResponse { RelatedEventId = @event.Id, Requested = DateTime.UtcNow, UserID = @event.UserId };
        db.SurveyResponses.Add(survey);
        await db.SaveChangesAsync();
        return survey.ID;
    }

    public async Task<List<User>> GetUsersWithActivity()
    {
        return await db.Users
            .Where(u => db.CopilotEventMetadataFiles.Where(e => e.RelatedChat.AuditEvent.User == u).Any() || db.CopilotEventMetadataMeetings.Where(e => e.RelatedChat.AuditEvent.User == u).Any())
            .ToListAsync();
    }

    /// <summary>
    /// Update the survey result with the initial score. Survey record should already exist as when asking the user about an event, we create an empty survey. 
    /// Returns the ID of the survey response updated.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If for some reason we can't find the existing survey by the copilot event</exception>
    public async Task<int> UpdateSurveyResultWithInitialScore(CommonAuditEvent @event, int score)
    {
        var response = await db.SurveyResponses.Where(e => e.RelatedEvent == @event).FirstOrDefaultAsync();
        if (response != null)
        {
            response.Rating = score;
            await db.SaveChangesAsync();
            return response.ID;
        }

        throw new ArgumentOutOfRangeException(nameof(@event));
    }

    public async Task<int> LogDisconnectedSurveyResult(int scoreGiven, string userUpn)
    {
        var user = await db.Users.Where(u => u.UserPrincipalName == userUpn).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User { UserPrincipalName = userUpn };
        }

        var survey = new UserSurveyResponse { Rating = scoreGiven, Requested = DateTime.UtcNow, User = user };
        db.SurveyResponses.Add(survey);
        await db.SaveChangesAsync();
        return survey.ID;
    }

    public async Task StopBotheringUser(string upn, DateTime until)
    {
        var user = await db.Users.Where(u => u.UserPrincipalName == upn).FirstOrDefaultAsync();
        if (user != null)
        {
            logger.LogInformation("User {upn} has been asked to stop bothering until {until}", upn, until);
            user.MessageNotBefore = until;
            await db.SaveChangesAsync();
        }
        else
        {
            logger.LogWarning("User {upn} not found", upn);
        }
    }

    public async Task LogSurveyFollowUp(int surveyId, SurveyFollowUpModel surveyFollowUp)
    {
        var survey = await db.SurveyResponses.Where(e => e.ID == surveyId).FirstOrDefaultAsync();
        if (survey != null)
        {
            survey.CopilotMakesMeMoreProductiveAgreeRating = surveyFollowUp.CopilotMakesMeMoreProductiveAgreeRating.HasValue ? (int)surveyFollowUp.CopilotMakesMeMoreProductiveAgreeRating.Value : null;
            survey.CopilotImprovesQualityOfWorkAgreeRating = surveyFollowUp.CopilotImprovesQualityOfWorkAgreeRating.HasValue ? (int)surveyFollowUp.CopilotImprovesQualityOfWorkAgreeRating.Value : null;
            survey.CopilotHelpsWithMundaneTasksAgreeRating = surveyFollowUp.CopilotHelpsWithMundaneTasksAgreeRating.HasValue ? (int)surveyFollowUp.CopilotHelpsWithMundaneTasksAgreeRating.Value : null;
            survey.CopilotAllowsTaskCompletionFasterAgreeRating = surveyFollowUp.CopilotAllowsTaskCompletionFasterAgreeRating.HasValue ? (int)surveyFollowUp.CopilotAllowsTaskCompletionFasterAgreeRating.Value : null;
            survey.Comments = surveyFollowUp.Comments;

            await db.SaveChangesAsync();
        }
    }
}
