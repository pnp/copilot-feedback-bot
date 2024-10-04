import { LogicalOperator, QuestionDatatype } from "../../apimodels/Enums";
import { SurveyQuestionDB } from "../../apimodels/Models";

// Common logic for assuming optional values
// That way we can do object comparisons without having to worry about undefined values
export function GetSurveyQuestionFromParams(question: string, questionId: string, forSurveyPageId: string, optimalAnswerValue?: string, op?: LogicalOperator, dataType?: QuestionDatatype)
    : SurveyQuestionDB {
    return {
        question: question,
        optimalAnswerLogicalOp: op ?? LogicalOperator.Equals,
        dataType: dataType ?? QuestionDatatype.String,
        optimalAnswerValue: optimalAnswerValue,
        id: undefined,
        forSurveyPageId: forSurveyPageId,
        questionId: questionId
    };
}

export function GetSurveyQuestionDB(partial: SurveyQuestionDB): SurveyQuestionDB {
    return GetSurveyQuestionFromParams(partial.question, partial.optimalAnswerLogicalOp, partial.forSurveyPageId, partial.optimalAnswerValue, partial.optimalAnswerLogicalOp, partial.dataType);
}

