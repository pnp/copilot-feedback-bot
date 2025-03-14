import moment, { Moment } from 'moment';
import { BaseDTO, BasicStats, ILoaderUsageStatsReportFilter, UsageStatsReport, ServiceConfiguration, SurveyPageDTO } from "../apimodels/Models";
import { BaseAxiosApiLoader } from "./AxiosApiLoader";


export const getClientConfig = async (loader: BaseAxiosApiLoader): Promise<ServiceConfiguration> => {
  return loader.loadFromApi('api/AppInfo/GetClientConfig', 'POST');
}

export const getUsageStatsReport = async (loader: BaseAxiosApiLoader): Promise<UsageStatsReport> => {

  const from: Moment = moment(new Date()).add(-1, 'years');
  const loaderUsageStatsReportFilter: ILoaderUsageStatsReportFilter = {
    from: moment(from).toDate(),
    to: new Date(),
    inDepartments: [],
    inCountries: []
  };

  return loader.loadFromApi('api/Stats/GetUsageStatsReport', 'POST', loaderUsageStatsReportFilter);
}

export const getBasicStats = async (loader: BaseAxiosApiLoader): Promise<BasicStats> => {
  return loader.loadFromApi('api/Stats/GetBasicStats', 'GET');
}


export const getSurveyPages = async (loader: BaseAxiosApiLoader): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'GET');
}

export const saveSurveyPage = async (loader: BaseAxiosApiLoader, updatedPage: SurveyPageDTO): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'POST', updatedPage);
}

export const deleteSurveyPage = async (loader: BaseAxiosApiLoader, pageId: string): Promise<null> => {
  const body: BaseDTO = { id: pageId };
  return loader.loadFromApi('api/SurveyQuestions/DeletePage', 'POST', body);
}

// Triggers
export const triggerSendSurveys = async (loader: BaseAxiosApiLoader): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/SendSurveys', 'POST');
}
export const triggerInstallBotForUser = async (loader: BaseAxiosApiLoader, upn: string): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/InstallBotForUser?upn=' + upn, 'POST');
}
export const triggerGenerateFakeActivityForUser = async (loader: BaseAxiosApiLoader, upn: string): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/GenerateFakeActivity?upn=' + upn, 'POST');
}
export const triggerGenerateFakeActivityForAllUsers = async (loader: BaseAxiosApiLoader): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/GenerateFakeActivityForAllUsers', 'POST');
}
export const triggerRefreshProfilingStats = async (loader: BaseAxiosApiLoader, weeksToKeep: number): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/RefreshProfilingStats?weeksToKeep=' + weeksToKeep, 'POST');
}
