import { LogicalOperator, QuestionDatatype } from "./Enums";

interface BaseDTOWithName extends BaseDTO { name: string }
interface BaseDTO { id?: string }

export interface IdTokenClaims {
  at_hash: string,
  country: string
  family_name: string,
  given_name: string
}

export interface SurveyPageDTO extends BaseDTOWithName {
  questions: SurveyQuestionDTO[];
  pageIndex: number;
  adaptiveCardTemplateJson: string;
  isPublished: boolean;
}

export interface SurveyQuestionDTO extends BaseDTO {
  questionId: string;
  forSurveyPageId: string;
  question: string;
  optimalAnswerValue?: string;
  dataType: QuestionDatatype;
  optimalAnswerLogicalOp? : LogicalOperator;
  index: number;
}
export interface BasicStats {
  usersSurveyed: number;
  usersResponded: number;
  usersNotResponded: number;
  usersFound: number;
}


export interface ServiceConfiguration {
  storageInfo: StorageInfo;
}

export interface StorageInfo {
  accountURI: string;
  sharedAccessToken: string;
}
