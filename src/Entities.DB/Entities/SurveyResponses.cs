using Entities.DB.Entities.AuditLog;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities;

[Table("survey_responses")]
public class UserSurveyResponseDB : UserRelatedEntity
{
    [Column("responded")]
    public DateTime Responded { get; set; }

    [Column("requested")]
    public DateTime Requested { get; set; }

    [Column("overrall_rating")]
    public int OverrallRating { get; set; }


    [ForeignKey(nameof(RelatedEvent))]
    [Column("related_audit_event_id")]
    public Guid? RelatedEventId { get; set; }
    public CommonAuditEvent? RelatedEvent { get; set; } = null;
}

/// <summary>
/// Associate a survey response with an activity type
/// </summary>
[Table("survey_response_activity_types")]
public class UserSurveyResponseActivityType : AbstractEFEntity
{

    [ForeignKey(nameof(UserSurveyResponse))]
    [Column("user_response_id")]
    public int UserSurveyResponseId { get; set; }
    public UserSurveyResponseDB UserSurveyResponse { get; set; } = null!;


    [ForeignKey(nameof(CopilotActivity))]
    [Column("copilot_activity_id")]
    public int CopilotActivityId { get; set; }
    public CopilotActivity CopilotActivity { get; set; } = null!;
}


[Table("survey_pages")]
public class SurveyPageDB : AbstractEFEntityWithName
{
    [Column("is_published")]
    public bool IsPublished { get; set; }
    public List<SurveyQuestionDB> Questions { get; set; } = new();

    [Column("index")]
    public int PageIndex { get; set; }

    [Column("template_json")]
    public string AdaptiveCardTemplateJson { get; set; } = null!;

}

[Table("survey_questions")]
public class SurveyQuestionDB : AbstractEFEntity
{
    [ForeignKey(nameof(SurveyPageDB))]
    [Column("for_SurveyPage_id")]
    public int ForSurveyPageId { get; set; }
    public SurveyPageDB ForSurveyPage { get; set; } = null!;

    [Column("question")]
    public required string Question { get; set; }

    [Column("optimal_answer_value")]
    public string? OptimalAnswerValue { get; set; } = null;

    [Column("optimal_answer_logical_op")]
    public LogicalOperator? OptimalAnswerLogicalOp { get; set; }

    [Column("data_type")]
    public required QuestionDatatype DataType { get; set; }

    [Column("index")]
    public int Index { get; set; }
}

[Table("survey_answers")]
public class SurveyAnswerDB : UserRelatedEntity
{
    [ForeignKey(nameof(ForQuestion))]
    [Column("for_question_id")]
    public int ForQuestionId { get; set; }
    public SurveyQuestionDB ForQuestion { get; set; } = null!;

    [Column("given_answer")]
    public required string GivenAnswer { get; set; }

    [Column("timestamp_utc")]
    public DateTime TimestampUtc { get; set; }
}


public enum QuestionDatatype
{
    Unknown,
    String,
    Int,
    Bool,
}

public enum LogicalOperator
{
    Unknown,
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
}
