import { QuestionDatatype } from "./Enums";

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
  usersLeague: EntityWithScore<ITrackedUser>[];
  departmentsLeague: EntityWithScore<string>[];
  countriesLeague: EntityWithScore<string>[];
}

export interface EntityWithScore<T> {
  entity: T;
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



interface SurveyQuestion<T> {
  optimalAnswer: T;
  optimalAnswerLogicalOp: LogicalOperator;
}

enum LogicalOperator {
  Unknown,
  Equals,
  NotEquals,
  GreaterThan,
  LessThan,
}

interface SurveyAnswer<T> {
  id: number;
  valueGiven: T;
  question: SurveyQuestion<T>;

  isPositiveResult: boolean;
}

interface StringSurveyAnswer extends SurveyAnswer<string> {
}

interface IntSurveyAnswer extends SurveyAnswer<number> {
}

interface BooleanSurveyAnswer extends SurveyAnswer<boolean> {
  
}

interface SurveyAnswersCollection {
  intSurveyAnswers: IntSurveyAnswer[];
  stringSurveyAnswers: StringSurveyAnswer[];
  booleanSurveyAnswers: BooleanSurveyAnswer[];
}

export interface SurveysReport {
  answers: SurveyAnswersCollection;
  percentageOfAnswersWithPositiveResult: number;
  stats: QuestionStats;
}

interface QuestionStats {
  highestPositiveAnswerQuestion: EntityWithScore<string>;
  highestNegativeAnswerQuestion: EntityWithScore<string>;
}
