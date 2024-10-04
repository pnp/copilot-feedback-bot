import { ServiceConfiguration, SurveyPageDB } from "../apimodels/Models";
import { BaseApiLoader } from "./ApiLoader";


export const getClientConfig = async (loader: BaseApiLoader): Promise<ServiceConfiguration> => {
  return loader.loadFromApi('api/AppInfo/GetClientConfig', 'POST')
    .then(async response => {
      const d: ServiceConfiguration = JSON.parse(response);
      return d;
    })
}

export const getSurveyPages = async (loader: BaseApiLoader): Promise<SurveyPageDB[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'GET')
    .then(async response => {
      const d: SurveyPageDB[] = JSON.parse(response);

      return d;
    })
}

export const saveSurveyPages = async (loader: BaseApiLoader, updatedPage: SurveyPageDB): Promise<SurveyPageDB[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'POST', updatedPage)
    .then(async response => {
      const d: SurveyPageDB[] = JSON.parse(response);

      return d;
    })
}
