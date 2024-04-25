using System.Text.Json.Serialization;

namespace Web.Bots;

public class SurveyInitialResponse
{
    [JsonPropertyName("rating")]
    public string Rating { get; set; } = null!;

    /// <summary>
    /// Parse the rating into a response
    /// </summary>
    public BotInitialReply? Response
    {
        get
        {
            var r = new BotInitialReply();
            if (Rating == BotConstants.SurveyAnswerRating1)
            {
                r.ScoreGiven = 1;
                r.Msg = "Oh no! Sorry to hear that.";
            }
            else if (Rating == BotConstants.SurveyAnswerRating2)
            {
                r.ScoreGiven = 2;
                r.Msg = "Sorry to hear that.";
            }
            else if (Rating == BotConstants.SurveyAnswerRating3)
            {
                r.ScoreGiven = 3;
                r.Msg = "Hopefully it'll be more useful next time";
            }
            else if (Rating == BotConstants.SurveyAnswerRating4)
            {
                r.IsHappy = true;
                r.ScoreGiven = 4;
                r.Msg = "Good! Happy to hear.";
            }
            else if (Rating == BotConstants.SurveyAnswerRating5)
            {
                r.IsHappy = true;
                r.ScoreGiven = 5;
                r.Msg = "Excellent! Very happy to hear.";
            }
            else
            {
                return null;
            }
            return r;
        }
    }
    public class BotInitialReply
    {
        public int ScoreGiven { get; set; }
        public bool IsHappy { get; set; } = false;
        public string Msg { get; set; } = null!;
    }
}
