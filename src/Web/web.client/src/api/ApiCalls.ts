import { BasicStats, ServiceConfiguration, SurveyPageDTO } from "../apimodels/Models";
import { BaseApiLoader } from "./ApiLoader";


export const getClientConfig = async (loader: BaseApiLoader): Promise<ServiceConfiguration> => {
  return loader.loadFromApi('api/AppInfo/GetClientConfig', 'POST')
    .then(async response => {
      const d: ServiceConfiguration = JSON.parse(response);
      return d;
    })
}

export const getBasicStats = async (loader: BaseApiLoader): Promise<BasicStats> => {
  return loader.loadFromApi('api/Stats/GetBasicStats', 'GET')
    .then(async response => {
      const d: BasicStats = JSON.parse(response);
      return d;
    })
}

export const getSurveyPages = async (loader: BaseApiLoader): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'GET')
    .then(async response => {
      const d: SurveyPageDTO[] = JSON.parse(response);

      return d;
    })
}

export const saveSurveyPages = async (loader: BaseApiLoader, updatedPage: SurveyPageDTO): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'POST', updatedPage)
    .then(async response => {
      const d: SurveyPageDTO[] = JSON.parse(response);

      return d;
    })
}

