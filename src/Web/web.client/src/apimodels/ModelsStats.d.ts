interface DashboardChartAndStats {
  skillsDataSummary: SkillsCube,
  companyOverviewStats: CompanyOverviewStats
}


interface CompanyOverviewStats {
  employeeCount: number,
  dataPoints: number,
  uniqueSkillsCount: number,
  lastRecordedSkillData?: Date,

  confidenceStats: ValueSetOverview,
  skillsLostStats: ValueSetOverview,
  totalValueStats: ValueSetOverview,
  currentValueTotal: number,
  currentConfidence: number,

}

interface ValueSetOverview {

  thisWeekValuePercentageChangeFromPrevious: number,
  thisMonthValuePercentageChangeFromPrevious: number,
  thisQuarterValuePercentageChangeFromPrevious: number,

  lastQuarterValueTotal: number,
  lastMonthValueTotal: number,
  lastWeekValueTotal: number
}

interface SkillsInsightsReports {
  skillsBySkillName: SkillsCube,
  skillsTrendAll: SkillsCube,
  skillsByJobTitle: SkillsCube,
  skillsStatsCompanyComparison: SkillsCompanyComparisonModel,
  skillsDeviation: SkillsDeviationModel,
  skillsPopularity: SkillsPopularityStats
}

interface SkillsCube {
  demographicStats: DemographicDatespanStatistics[],
  metadata: DatasetMetadata
}

interface DatasetMetadata {

  allTimeStats: DatasetStats,
  filteredStats: DatasetStats,
  datesCovered: Date[]
}
interface DatasetStats {
  highestValue?: ValueInTime,
  lowestValue?: ValueInTime,
  percentDiffInView: number
}

interface ValueInTime {
  value: number,
  when: Date
}
interface SkillsDeviationModel {
  skillsGoingUp: DemographicDatespanStatistics[],
  flatLineSkills: DemographicDatespanStatistics[],
  topValuedSkills: DemographicDatespanStatistics[],
  skillStandardDeviations: DemographicStandardDeviationDatespanStatistics
}
interface ActiveAndInactiveSkillsDateSpanStatistics {
  activeStats: ValueForDateSpan[],
  inactiveStats: ValueForDateSpan[]
}
interface DemographicDatespanStatistics extends ActiveAndInactiveSkillsDateSpanStatistics {
  demographicName: string,
}
interface ValueForDateSpan {
  from: Date,
  to: Date,
  skillValueOnDate: SkillsValue
}

interface SkillsValue {
  value: number;
  confidence: number;
}

interface SkillsCompanyComparisonModel {
  companyStats: CompanyComparisonScore[],
  allDates: Date[],
}
interface CompanyComparisonScore {
  name: string,
  skillValuesAllDates: number[]
}


interface SkillsNameStats {
  stats: SkillStat[]
}
interface SkillStat extends AbstractEFEntityWithName {
  totalValue: number
}

interface InitiativesSummary {
  reports: SkillsInitiativeReport[]
  totalSkillsValue: number;
}

interface SkillsInitiativeReport {
  totalPercentComplete: number;
  percentIncreaseFromStart: number;
  roi: number;
  skillProgress: SkillProgress[]
  skillsInitiative: ClientSkillsInitiative
}

interface SkillProgress {
  percentIncreaseFromStart: number;
  percentCompleteToTarget: number,
  targetAverageLevel: number,
  skillName: SkillName,
  initialAverageValue: number,
  currentAverageLevel: number
  initialTotalCashValue: number,
  currentTotalCashLevel: number,
  employeeContributionBreakdown: EmployeeProgressForSkill[]
}

interface EmployeeProgressForSkill {
  employeeId: string,
  averageLevelStart: number,
  averageLevelEnd: number
  percentageIncrease: number
}
