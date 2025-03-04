using Common.Engine.Surveys;
using Common.Engine.Surveys.Model;
using Entities.DB.Entities;
using Entities.DB.Entities.AuditLog;

namespace UnitTests.FakeLoaderClasses;

internal class FakeSurveyProcessor : ISurveyEventsProcessor
{
    public Task ProcessSurveyRequest(SurveyPendingActivities activities)
    {
        Console.WriteLine("Fake processing survey request");
        return Task.CompletedTask;
    }
}

internal class FakeSurveyManagerDataLoader : ISurveyManagerDataLoader
{
    private readonly TestsConfig _testsConfig;

    static Guid ID_FILE = Guid.NewGuid();
    static Guid ID_MEETING = Guid.NewGuid();

    private List<Guid> _ids = new List<Guid>();

    public FakeSurveyManagerDataLoader(TestsConfig testsConfig)
    {
        _testsConfig = testsConfig;
    }

    public Task<DateTime?> GetLastUserSurveyDate(User user)
    {
        return Task.FromResult<DateTime?>(null);
    }

    public Task<List<BaseCopilotSpecificEvent>> GetUnsurveyedActivities(User user, DateTime? from)
    {
        var list = new List<BaseCopilotSpecificEvent>();

        if (!_ids.Contains(ID_FILE))
        {
            list.Add(new CopilotEventMetadataFile
            {
                FileName = new SPEventFileName { Name = _testsConfig.TeamSitesFileName },
                FileExtension = new SPEventFileExtension { Name = _testsConfig.TeamSiteFileExtension },
                Url = new Entities.DB.Entities.SP.Url { FullUrl = _testsConfig.TeamSiteFileUrl },
                RelatedChat = new CopilotChat
                {
                    AppHost = "unit",
                    AuditEvent = new CommonAuditEvent
                    {
                        Id = ID_FILE,
                        User = user
                    }
                },
            });
        }

        if (!_ids.Contains(ID_MEETING))
        {
            list.Add(new CopilotEventMetadataMeeting
            {
                RelatedChat = new CopilotChat
                {
                    AppHost = "unit",
                    AuditEvent = new CommonAuditEvent
                    {
                        Id = ID_MEETING,
                        User = user,
                        TimeStamp = DateTime.Now.AddDays(-1)
                    }
                },
            });
        }
        return Task.FromResult(list);
    }

    public Task<User> GetUser(string upn)
    {
        return Task.FromResult(new User { UserPrincipalName = upn });
    }

    public Task<List<User>> GetUsersWithActivity()
    {
        return Task.FromResult(new List<User> { new User { UserPrincipalName = "testupn" } });
    }

    public Task<int> UpdateSurveyResultWithInitialScore(CommonAuditEvent @event, int score)
    {
        Console.WriteLine($"Fake user updated survey result for {@event.Id} with score {score}");
        return Task.FromResult(1);
    }

    public Task StopBotheringUser(string upn, DateTime until)
    {
        Console.WriteLine($"Fake user requested stopping bothering until {until}");
        return Task.CompletedTask;
    }

    public Task<int> LogDisconnectedSurveyResultWithInitialScore(int scoreGiven, string userUpn)
    {
        Console.WriteLine($"Fake user logged disconnected survey result with score {scoreGiven}");
        return Task.FromResult(1);
    }

    Task<int> ISurveyManagerDataLoader.LogSurveyRequested(CommonAuditEvent @event)
    {
        _ids.Add(@event.Id);
        return Task.FromResult(1);
    }

    public Task<List<SurveyAnswerDB>> SaveAnswers(User user, List<SurveyPageUserResponse.RawResponse> answers, int existingSurveyId)
    {
        Console.WriteLine($"Fake user saved answers for {user.UserPrincipalName}");
        return Task.FromResult(new List<SurveyAnswerDB>());
    }

    public Task<List<SurveyPageDB>> GetSurveyPages(bool publishedOnly)
    {
        return Task.FromResult(new List<SurveyPageDB>()
        {
            new SurveyPageDB
            {
                Questions = new List<SurveyQuestionDB>
                {
                    new SurveyQuestionDB
                    {
                        ID = 1,
                        Question = "Question 1",
                        DataType = QuestionDatatype.Int
                    },
                    new SurveyQuestionDB
                    {
                        ID = 2,
                        Question = "Question 2",
                        DataType = QuestionDatatype.String
                    }
                },
                IsPublished = publishedOnly
            } });
    }

    public Task<bool> DeleteSurveyPage(int id)
    {
        throw new NotImplementedException();
    }

    public Task SaveSurveyPage(SurveyPageDTO pageUpdate)
    {
        throw new NotImplementedException();
    }
}
