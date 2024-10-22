using Newtonsoft.Json;

namespace Common.Engine.Surveys.Model;

/// <summary>
/// Representation of a user's response to a survey page
/// </summary>
public class SurveyPageUserResponse
{
    public SurveyPageUserResponse(string? jsonResultFromAdaptiveCard, string? userPrincipalName)
    {
        if (string.IsNullOrEmpty(userPrincipalName) || string.IsNullOrEmpty(jsonResultFromAdaptiveCard))
        {
            IsValid = false;
            return;
        }
        try
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResultFromAdaptiveCard);
            IsValid = true;

            if (values == null)
            {
                throw new SurveyEngineDataException("Failed to deserialize survey response");
            }
            foreach (var kvp in values)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key))
                {
                    var id = kvp.Key.Replace("autoQuestionId-", "");
                    if (int.TryParse(id, out var questionId))
                    {
                        var a = new RawResponse { QuestionId = questionId, Response = kvp.Value };
                        Answers.Add(a);
                    }
                }
            }
        }
        catch (JsonReaderException)
        {
            IsValid = false;
            return;
        }

        IsValid = !string.IsNullOrEmpty(userPrincipalName);
        UserPrincipalName = userPrincipalName;
    }

    public bool IsValid { get; }

    public string UserPrincipalName { get; set; } = string.Empty;
    public List<RawResponse> Answers { get; set; } = new();
    public List<int> QuestionIds => Answers.Select(a => a.QuestionId).ToList();

    public record RawResponse
    {
        public required int QuestionId { get; set; }
        public required string Response { get; set; }
    }
}
