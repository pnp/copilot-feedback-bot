import { LogicalOperator, QuestionDatatype } from "../../apimodels/Enums";
import { SurveyQuestionDTO } from "../../apimodels/Models";

// Common logic for assuming optional values
// That way we can do object comparisons without having to worry about undefined values
export function GetSurveyQuestionFromParams(question: string, index: number, questionId: string, forSurveyPageId: string, 
    id?: string, optimalAnswerValue?: string, op?: LogicalOperator, dataType?: QuestionDatatype)
    : SurveyQuestionDTO {
    return {
        id: id,
        index: index,
        question: question,
        optimalAnswerLogicalOp: op ?? LogicalOperator.Equals,
        dataType: dataType ?? QuestionDatatype.String,
        optimalAnswerValue: optimalAnswerValue,
        forSurveyPageId: forSurveyPageId,
        questionId: questionId
    };
}

export function GetSurveyQuestionDTO(partial: SurveyQuestionDTO): SurveyQuestionDTO {
    return GetSurveyQuestionFromParams(partial.question, partial.index, partial.questionId, partial.forSurveyPageId, 
        partial.id, partial.optimalAnswerValue, partial.optimalAnswerLogicalOp, partial.dataType);
}

