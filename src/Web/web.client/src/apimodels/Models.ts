import { LogicalOperator, QuestionDatatype } from "./Enums";

interface BaseDTOWithName extends BaseDTO { name: string }
export interface BaseDTO { id?: string }

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

export interface UsageStatsReport {
  uniqueActivities: string[];
  dates: Date[];
  users: IUserWithScore[];
}

export interface IUserWithScore {
  user: ITrackedUser;
  score: number;
}

export interface ITrackedUser {
  companyName?: string;
  department?: string;
  jobTitle?: string;
  officeLocation?: string;
  stateOrProvince?: string;
  usageLocation?: string;
  userCountry?: string;
  licenses: string[];
  manager?: ITrackedUser;
  userPrincipalName: string;
}

export interface ILoaderUsageStatsReportFilter extends IUsageStatsReportFilter {
  from: Date; // Equivalent of DateOnly in C#
  to: Date;   // Equivalent of DateOnly in C#
}

export interface IUsageStatsReportFilter {
  inDepartments: string[];
  inCountries: string[];
}

export interface ServiceConfiguration {
  storageInfo: StorageInfo;
}

export interface StorageInfo {
  accountURI: string;
  sharedAccessToken: string;
}
