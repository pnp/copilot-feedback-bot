import { BaseDTO, BasicStats, ServiceConfiguration, SurveyPageDTO } from "../apimodels/Models";
import { BaseAxiosApiLoader } from "./AxiosApiLoader";


export const getClientConfig = async (loader: BaseAxiosApiLoader): Promise<ServiceConfiguration> => {
  return loader.loadFromApi('api/AppInfo/GetClientConfig', 'POST')
    .then(async response => {
      const d: ServiceConfiguration = JSON.parse(response);
      return d;
    })
}

export const getBasicStats = async (loader: BaseAxiosApiLoader): Promise<BasicStats> => {
  return loader.loadFromApi('api/Stats/GetBasicStats', 'GET')
    .then(async response => {
      const d: BasicStats = JSON.parse(response);
      return d;
    })
}

export const getSurveyPages = async (loader: BaseAxiosApiLoader): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'GET')
    .then(async response => {
      const d: SurveyPageDTO[] = JSON.parse(response);

      return d;
    })
}

export const saveSurveyPage = async (loader: BaseAxiosApiLoader, updatedPage: SurveyPageDTO): Promise<SurveyPageDTO[]> => {
  return loader.loadFromApi('api/SurveyQuestions', 'POST', updatedPage)
    .then(async response => {
      const d: SurveyPageDTO[] = JSON.parse(response);

      return d;
    })
}

export const deleteSurveyPage = async (loader: BaseAxiosApiLoader, pageId: string): Promise<null> => {
  const body: BaseDTO = { id: pageId };
  return loader.loadFromApi('api/SurveyQuestions/DeletePage', 'POST', body)
    .then(() => {
      return null;
    })
}


export const triggerSendSurveys = async (loader: BaseAxiosApiLoader): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/SendSurveys', 'POST')
    .then(() => {
      return null;
    })
}
export const triggerInstallBotForUser = async (loader: BaseAxiosApiLoader, upn: string): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/InstallBotForUser?upn=' + upn, 'POST')
    .then(() => {
      return null;
    })
}
export const triggerGenerateFakeActivityForUser = async (loader: BaseAxiosApiLoader, upn: string): Promise<null> => {
  return loader.loadFromApi('/api/Triggers/GenerateFakeActivity?upn=' + upn, 'POST')
    .then(() => {
      return null;
    })
}
