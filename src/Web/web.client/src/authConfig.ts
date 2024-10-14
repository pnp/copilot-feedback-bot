export const msalConfig = {
  auth: {
    clientId: String(import.meta.env.VITE_MSAL_CLIENT_ID),
    authority: String(import.meta.env.VITE_MSAL_AUTHORITY),
  },
  cache: {
    cacheLocation: "sessionStorage", // This configures where your cache will be stored
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  }
};

export const loginRequest = {
  scopes: [String(import.meta.env.VITE_MSAL_SCOPES)]
};

export const teamsAppConfig = {
  startLoginPageUrl: String(import.meta.env.VITE_TEAMSFX_START_LOGIN_PAGE_URL),
  apiEndpoint: String(import.meta.env.VITE_API_ENDPOINT),
}
