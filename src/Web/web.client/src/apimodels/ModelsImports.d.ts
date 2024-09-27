interface ImportSession<T> {
  rowKey: string;
  results?: T;
  startedUtc?: Date;
  fileImportStartedUtc?: Date;
  fileImportFinishedUtc?: Date;
  errors?: string
}

interface SkillsResolutionResult {
  changeSuggestions: SkillSuggestions[];
  unresolvedSkills: string[];
  autoImportedSkills: SkillsDataImportItem[];
}

interface SkillSuggestions {
  original: string;
  suggestions: string[];
}
interface SkillPick {
  original: string;
  picked: string;
}

interface SkillsDataImportItem
{
  skillName: string;
}