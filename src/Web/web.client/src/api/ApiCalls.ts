import { BaseApiLoader } from "./ApiLoader";


export const getClientConfig = async (loader: BaseApiLoader): Promise<ServiceConfiguration> => {
  return loader.loadFromApi('api/AppInfo/GetClientConfig', 'POST')
    .then(async response => {
      const d: ServiceConfiguration = JSON.parse(response);
      return d;
    })
}

export interface ServiceConfiguration {
  storageInfo: StorageInfo;
}

interface StorageInfo {
  accountURI: string;
  sharedAccessToken: string;
}