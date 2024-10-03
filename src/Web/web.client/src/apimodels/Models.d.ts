
abstract interface AbstractEFEntityWithName extends AbstractEFEntity { name: string }
abstract interface AbstractEFEntity { id?: string }

interface IdTokenClaims {
  at_hash: string,
  country: string
  family_name: string,
  given_name: string
}

interface SurveyPageDB extends AbstractEFEntityWithName {
  questions: SurveyQuestionDB[];
  pageIndex: number;
  adaptiveCardTemplateJson: string;
  isPublished: boolean;
}

interface SurveyPageEditViewModel extends SurveyPageDB {
  adaptiveCardTemplateJsonWithQuestions: string;
}

interface SurveyQuestionDB extends AbstractEFEntity {
  questionId: string;
  forSurveyPageId: string;
  question: string;
  optimalAnswerValue?: string;
  dataType: QuestionDatatype;
  optimalAnswerLogicalOp? : LogicalOperator;
}


export interface ServiceConfiguration {
  storageInfo: StorageInfo;
}

interface StorageInfo {
  accountURI: string;
  sharedAccessToken: string;
}
