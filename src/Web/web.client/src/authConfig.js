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

// Add scopes here for ID token to be used at Microsoft identity platform endpoints.
export const loginRequest = {
  scopes: [String(import.meta.env.VITE_MSAL_SCOPES)]
};

