import { MoveNodeParams } from "../components/common/controls/SkillsTree";
import { loadFromApi } from "./ApiLoader";

// Reports
export const loadSkillsInsightsReports = async (filter: ToFromWithSkillIdModelLoadFilter, token: string): Promise<SkillsInsightsReports> => {
  return loadFromApi('Charts/SkillsInsightsReports', 'POST', token, filter)
    .then(async response => {
      const d: SkillsInsightsReports = JSON.parse(response);
      return d;
    });
}

export const loadCompanyComparisonStats = async (filter: ToFromModelLoadFilter, token: string): Promise<SkillsCompanyComparisonModel> => {
  return loadFromApi('Charts/SkillsStatsCompanyComparison', 'POST', token, filter)
    .then(async response => {
      const d: SkillsCompanyComparisonModel = JSON.parse(response);
      return d;
    });
}
export const loadSkillsNamePopularityStats = async (filter: SkillPopularityFilter, token: string): Promise<SkillsNameStats> => {
  return loadFromApi('Charts/SkillsNamePopularityStats', 'POST', token, filter)
    .then(async response => {
      const d: SkillsNameStats = JSON.parse(response);
      return d;
    });
}

export const loadDashboardChartAndStats = async (filter: ToFromWithSkillIdModelLoadFilter, token: string): Promise<DashboardChartAndStats> => {
  return loadFromApi('Stats/DashboardChartAndStats', 'POST', token, filter)
    .then(async response => {
      const d: DashboardChartAndStats = JSON.parse(response);
      return d;
    });
}

// Trends & high-level stats
export const loadCompanyOverviewStats = async (token: string): Promise<CompanyOverviewStats> => {
  return loadFromApi('Stats/CompanyOverviewStats', 'GET', token)
    .then(async response => {
      const d: CompanyOverviewStats = JSON.parse(response);
      return d;
    });
}

// Misc
export const loadUserProfile = async (token: string): Promise<LoginProfile | null> => {
  return loadFromApi('Users/GetLoggedInUser', 'GET', token)
    .then(async response => {
      const userContext: UserProfileResponse = JSON.parse(response);
      if (!userContext.login) {
        console.warn(`User has no profile`);
      }
      return userContext.login;
    });
}

// Lookups
export const loadJobTitles = async (token: string): Promise<LookupWithEmployeeCount<JobTitle>[]> => {
  return loadFromApi('Lookups/JobTitles', 'GET', token)
    .then(async response => {
      const data: LookupWithEmployeeCount<JobTitle>[] = JSON.parse(response);
      return data;
    })
}

export const loadSkillsTree = async (token: string): Promise<SkillName[]> => {

  return loadFromApi('Lookups/SkillsTree', 'GET', token)
    .then(async response => {
      const data: SkillName[] = JSON.parse(response);
      return data;
    })
}

export const loadFileTypeNames = async (token: string): Promise<string[]> => {
  return loadFromApi('Lookups/FileTypeNames', 'GET', token)
    .then(async response => {
      const d: string[] = JSON.parse(response);
      return d;
    })
}

export const moveSkillNode = async (token: string, arg: MoveNodeParams): Promise<SkillName[]> => {
  return loadFromApi('Lookups/MoveSkillInTree', 'POST', token, arg)
    .then(async response => {
      const data: SkillName[] = JSON.parse(response);
      return data;
    })
}

// Imports
export const loadImportLogs = async (token: string): Promise<ImportLog[]> => {
  return loadFromApi('Imports/GetImportLogs', 'GET', token)
    .then(async response => {
      const d: ImportLog[] = JSON.parse(response);
      return d;
    })
}
export const deleteImportLog = async (token: string, id: string): Promise<null> => {
  return loadFromApi('Imports/DeleteBatch?importId=' + id, 'DELETE', token)
    .then(async () => {
      return Promise.resolve(null);
    })
}

export const startNewImport = async (token: string, file: File, fileFormat: string, fullImport: boolean): Promise<ImportSession<SkillsResolutionResult>> => {

  const formData = new FormData();
  formData.append("File", file, file.name);

  var req: any = {
    method: "POST",
    headers: {
      'Authorization': 'Bearer ' + token,
    },
    body: formData
  };

  const skillImportTypeName: string = fullImport === true ? "Full" : "Incremental";
  const url = `Imports/StartImport?formatName=${fileFormat}&skillImportTypeName=${skillImportTypeName}`;
  console.info(`Loading ${url}...`);
  return fetch(url, req)
    .then(async response => {

      if (response.ok) {
        const dataText: string = await response.text();
        console.info(`${url}: '${dataText}'`);
        const d: ImportSession<SkillsResolutionResult> = JSON.parse(dataText);
        return d;
      }
      else {
        const dataText: string = await response.text();
        const errorTitle = `Error ${response.status} POSTing to/from API '${url}'`;

        if (dataText !== "")
          alert(`${errorTitle}: ${dataText}`)
        else
          alert(errorTitle);

        return Promise.reject();
      }
    });
}

export const getImportStatus = async (token: string, sessionId: string): Promise<ImportSession<SkillsResolutionResult>> => {
  return loadFromApi('Imports/GetImportStatus?sessionId=' + sessionId, 'POST', token)
    .then(async response => {
      const d: ImportSession<SkillsResolutionResult> = JSON.parse(response);
      return d;
    })
}

// ImportConfigurations
export const loadImportConfigurations = async (token: string): Promise<ImportConfiguration[]> => {
  return loadFromApi('ImportConfigurations', 'GET', token)
    .then(async response => {
      const d: ImportConfiguration[] = JSON.parse(response);
      return d;
    })
}
export const saveImportConfigurations = async (token: string, update: ImportConfiguration): Promise<null> => {
  return loadFromApi('ImportConfigurations', 'POST', token, update)
    .then(() => {
      return Promise.resolve(null);
    })
}
export const deleteImportConfigurations = async (token: string, id: string): Promise<null> => {
  return loadFromApi('ImportConfigurations?id=' + id, 'DELETE', token)
    .then(() => {
      return Promise.resolve(null);
    })
}

// Initiatives
export const getSkillsInitiatives = async (token: string): Promise<InitiativesSummary> => {
  return loadFromApi('SkillsInitiatives', 'GET', token)
    .then(async response => {
      const d: InitiativesSummary = JSON.parse(response);
      return d;
    })
}
export const addEditSkillsInitiative = async (token: string, req: CreateEditInitiativeRequest): Promise<null> => {
  return loadFromApi('SkillsInitiatives', 'POST', token, req)
    .then(() => {
      return Promise.resolve(null);
    })
}
export const deleteSkillsInitiative = async (token: string, id: string): Promise<null> => {
  return loadFromApi('SkillsInitiatives?id=' + id, 'DELETE', token)
    .then(() => {
      return Promise.resolve(null);
    })
}

export const findSkillsLightCast = async (token: string, q: string): Promise<NameAndId[]> => {
  return loadFromApi('Lookups/FindJobTitle?q=' + encodeURIComponent(q), 'GET', token)
    .then(async response => {
      const d: NameAndId[] = JSON.parse(response);
      return d;
    })
}

export const fullLoadSkillLightCast = async (token: string, id: string): Promise<JobTitleFull> => {
  return loadFromApi('Lookups/GetJobTitle?id=' + id, 'GET', token)
    .then(async response => {
      const d: JobTitleFull = JSON.parse(response);
      return d;
    })
}

export const getJobTitlesReport = async (token: string, reportId: string): Promise<JobTitleReportResponse> => {
  return loadFromApi('Reports/GetJobTitlesReport?id=' + reportId, 'GET', token)
    .then(async response => {
      const d: JobTitleReportResponse = JSON.parse(response);
      return d;
    })
}
export const startProcessingJobTitlesReport = async (token: string, skillIds: string[]): Promise<JobTitleReportResponse> => {
  return loadFromApi('Reports/StartProcessingJobTitlesReport', 'POST', token, skillIds)
    .then(async response => {
      const d: JobTitleReportResponse = JSON.parse(response);
      return d;
    })
}
