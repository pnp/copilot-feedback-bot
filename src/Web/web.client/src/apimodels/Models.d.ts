
abstract interface AbstractEFEntityWithName extends AbstractEFEntity { name: string }
abstract interface AbstractEFEntity { id: string }

// Dup?
interface NameAndId {
  name: string;
  id: string
}

interface LookupWithEmployeeCount<T extends NameAndId> {
  item: T;
  employeeCount: number;
}

interface IdTokenClaims {
  at_hash: string,
  country: string
  family_name: string,
  given_name: string
}
interface UserProfileResponse {
  login: LoginProfile
}
interface LoginProfile {
  client: Client
}
interface User {
  emailAddress: string,
  status: string,
  inviting: boolean,
  inviteSent: boolean,
}
interface Client {
  name: string,
  dnsLabel: string,
  guid: string
}
interface ToFromModelLoadFilter {
  from: Date,
  to: Date
}

interface ToFromWithSkillIdModelLoadFilter extends ToFromModelLoadFilter {
  skillsIdFilter?: string
}

interface SkillPopularityFilter extends ToFromModelLoadFilter {
  topSkills?: number,
  bottomSkills?: number
}
interface ImportLog extends AbstractEFEntityWithName {
  importedOn: Date,
  newDataPoints: number,
  source: ImportSource,
}

interface ImportConfiguration extends AbstractEFEntityWithName {
  firstLineIsHeader: boolean,
  ratingColumnIdx: number,
  ratingTypeColumnIdx: number,
  ratingSourceColumnIdx: number,
  ratingDateColumnIdx: number,
  employeeIdColumnIdx: number,
  jobTitleColumnIdx: number,
  jobFamilyColumnIdx: number,
  jobFamilyGroupColumnIdx: number,
  skillNameColumnIdx: number,
  skillPlanColumnIdx: number,
  maxSkillRating: number
}

interface ImportSource extends AbstractEFEntityWithName { }


interface SkillsNameStats {
  stats: SkillStat[]
}
interface SkillStat extends AbstractEFEntityWithName {
  totalValue: number
}

interface JobTitle extends AbstractEFEntityWithName { }

interface SkillName extends AbstractEFEntityWithName {
  children: SkillName[]
}

interface ClientSkillsInitiative extends AbstractEFEntityWithName {
  cost: number;
  start: Date;
  end: Date;
  name: string;
  targetAudiences: InitiativeScopeTarget[];
  targetSkills: InitiativeSkillTarget[];
}
interface InitiativeScopeTarget extends AbstractEFEntity {
  jobTitle: JobTitle;
}

interface InitiativeSkillTarget extends AbstractEFEntity {
  skill: SkillName;
  targetAverage: number;
}

interface LightCastConfig {
  ClientId: string;
  Secret: string;
}


interface JobTitleFull extends NameAndId {
  mapping?: Mapping
}

interface Mapping {
  names: string[],
  skills: string[]
  socs: NameAndId[]
}

interface JobTitleReportResponse {
  reportId: string,
  completed: Date,
  report?: JobTitleReport
}

interface JobTitleReport {
  jobTitlesAndSkills: JobTitleSkills[],
  skillNameAndDescriptions: SkillNameAndDescription[]
}

interface JobTitleSkills {
  jobTitleName: string,
  skills: string[]
}
interface SkillNameAndDescription {
  name: string,
  description: string
}
