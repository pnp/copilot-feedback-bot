import { readConfigVal } from "./utils/configReader";

export const msalConfig = {
  auth: {
    clientId: readConfigVal("MSAL_CLIENT_ID"),
    authority: readConfigVal("MSAL_AUTHORITY"),
  },
  cache: {
    cacheLocation: "sessionStorage", // This configures where your cache will be stored
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  }
};

export const loginRequest = {
  scopes: [readConfigVal("MSAL_SCOPES")]
};

export const teamsAppConfig = {
  startLoginPageUrl: readConfigVal("TEAMSFX_START_LOGIN_PAGE_URL"),
}
