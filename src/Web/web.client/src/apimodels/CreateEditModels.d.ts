
interface CreateEditInitiativeRequest {
  id?: string;
  cost: number;
  start: Date;
  end: Date;
  name: string;
  targetAudiences: CreateEditInitiativeScopeTarget[];
  targetSkills: CreateEditInitiativeSkillTarget[];
}

interface CreateEditInitiativeScopeTarget {
  jobTitleId: string;
}

interface CreateEditInitiativeSkillTarget {
  skillId: string;
  targetAverage: number;
}
