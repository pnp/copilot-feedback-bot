import { BaseDTO, BasicStats, ServiceConfiguration, SurveyPageDTO } from "../apimodels/Models";
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

export const saveSurveyPage = async (loader: BaseApiLoader, updatedPage: SurveyPageDTO): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'POST', updatedPage)
    .then(async response => {
      const d: SurveyPageDTO[] = JSON.parse(response);

      return d;
    })
}

export const deleteSurveyPage = async (loader: BaseApiLoader, pageId: string): Promise<null> => {
  const body: BaseDTO = { id: pageId };
  return loader.loadFromApi('api/SurveyQuestions/DeletePage', 'POST', body)
    .then(() => {
      return null;
    })
}


export const triggerSendSurveys = async (loader: BaseApiLoader): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/SendSurveys', 'POST')
    .then(() => {
      return null;
    })
}
export const triggerInstallBotForUser = async (loader: BaseApiLoader, upn: string): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/InstallBotForUser?upn=' + upn, 'POST')
    .then(() => {
      return null;
    })
}
export const triggerGenerateFakeActivityForUser = async (loader: BaseApiLoader, upn: string): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/GenerateFakeActivity?upn=' + upn, 'POST')
    .then(() => {
      return null;
    })
}
