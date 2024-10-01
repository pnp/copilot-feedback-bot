
abstract interface AbstractEFEntityWithName extends AbstractEFEntity { name: string }
abstract interface AbstractEFEntity { id: string }

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
}

interface SurveyQuestionDB extends AbstractEFEntity {
  questionId: string;
  question: string;
  optimalAnswerValue?: string;
  DataType: QuestionDatatype;
}

enum QuestionDatatype {
  Unknown = 0,
  String = 1,
  Int = 2,
  Bool = 3,
}

export interface ServiceConfiguration {
  storageInfo: StorageInfo;
}

interface StorageInfo {
  accountURI: string;
  sharedAccessToken: string;
}
