import { ServiceConfiguration, SurveyPageEditViewModel } from "../apimodels/Models";
import { BaseApiLoader } from "./ApiLoader";


export const getClientConfig = async (loader: BaseApiLoader): Promise<ServiceConfiguration> => {
  return loader.loadFromApi('api/AppInfo/GetClientConfig', 'POST')
    .then(async response => {
      const d: ServiceConfiguration = JSON.parse(response);
      return d;
    })
}

export const getSurveyPages = async (loader: BaseApiLoader): Promise<SurveyPageEditViewModel[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'GET')
    .then(async response => {
      const d: SurveyPageEditViewModel[] = JSON.parse(response);
      return d;
    })
}
