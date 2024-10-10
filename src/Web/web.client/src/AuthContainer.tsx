import React, { PropsWithChildren, useContext, useState } from 'react';

import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { BaseApiLoader, MsalApiLoader, TeamsSsoApiLoader } from './api/ApiLoader';
import { AppRoutes, LoginMethod } from './AppRoutes';
import { msalConfig, loginRequest } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { BearerTokenAuthProvider, createApiClient, ErrorWithCode, TeamsUserCredential } from '@microsoft/teamsfx';

import * as axios from "axios";


async function callFunction(teamsUserCredential: TeamsUserCredential) {
    try {
      const apiBaseUrl = "https://localhost:7053" + "/api/";
      // createApiClient(...) creates an Axios instance which uses BearerTokenAuthProvider to inject token to request header
      const apiClient = createApiClient(
        apiBaseUrl,
        new BearerTokenAuthProvider(async () => (await teamsUserCredential.getToken(""))!.token)
      );
      const response = await apiClient.get("ass");
      return response.data;
    } catch (err: unknown) {
      if (axios.default.isAxiosError(err)) {
        let funcErrorMsg = "";
  
        if (err?.response?.status === 404) {
          funcErrorMsg = `There may be a problem with the deployment of Azure Functions App, please deploy Azure Functions (Run command palette "Teams: Deploy") first before running this App`;
        } else if (err.message === "Network Error") {
          funcErrorMsg =
            "Cannot call Azure Functions due to network error, please check your network connection status and ";
          if (err.config?.url && err.config.url.indexOf("localhost") >= 0) {
            funcErrorMsg += `make sure to start Azure Functions locally (Run "npm run start" command inside api folder from terminal) first before running this App`;
          } else {
            funcErrorMsg += `make sure to provision and deploy Azure Functions (Run command palette "Teams: Provision" and "Teams: Deploy") first before running this App`;
          }
        } else {
          funcErrorMsg = err.message;
          if (err.response?.data?.error) {
            funcErrorMsg += ": " + err.response.data.error;
          }
        }
  
        throw new Error(funcErrorMsg);
      }
      throw err;
    }
  }

  
export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    const [apiLoader, setApiLoader] = useState<BaseApiLoader | undefined>();
    const [msalInitialised, setMsalInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const setLoginMethod = React.useCallback((method: LoginMethod) => { props.loginMethodChange(method) }, []);

    const teamsUserCredential = useContext(TeamsFxContext).teamsUserCredential;


    
    // Figure out if we can use Teams SSO or MSAL
    const initAuth = React.useCallback(() => {

        if (teamsUserCredential) {
            console.debug("We have Teams credentials. Trying SSO...");
            
            callFunction(teamsUserCredential);
        }
        else {
            console.debug("No Teams credentials. Using MSAL");
            initMsal();
        }
    }, [teamsUserCredential]);

    const [authCount, setAuthCount] = React.useState(0);

    const forceRerender = React.useCallback(() => {
        setAuthCount(authCount + 1);
        console.log("forceRerender: ", authCount);
    }, [authCount]);


    React.useEffect(() => {
        initAuth();
    }, [teamsUserCredential, initAuth]);

    const initMsal = React.useCallback(() => {
        const initializeMsal = async () => {
            await instance.initialize();    // Initialize MSAL instance
            setMsalInitialised(true);
            console.debug("MSAL initialised with client ID: " + msalConfig.auth.clientId);
        };

        initializeMsal();
    }, [apiLoader, msalInitialised]);

    // Init MSAL API loader if we have an account
    React.useEffect(() => {

        if (msalInitialised && isAuthenticated && !apiLoader) {
            console.debug("Creating MSAL API loader");

            const loader = new MsalApiLoader(instance, accounts);
            setApiLoader(loader);
            props.onApiLoaderReady(loader);
        }
        else {
            if (msalInitialised) {
                if (!isAuthenticated)
                    console.info("MSAL initialised, but not authenticated yet. Will need to login to Entra ID via login page");
            }
            else
                console.warn("MSAL not initialised yet...check configuration");
        }
    }, [msalInitialised, isAuthenticated]);

    const authReload = React.useCallback(() => {
        console.debug("Auth reload requested");
        forceRerender();
    }, []);

    return (
        <AppRoutes apiLoader={apiLoader} loginMethod={props.loginMethod} onAuthReload={authReload}>{props.children}</AppRoutes>
    );
}


export interface AuthContainerProps {
    onApiLoaderReady: Function;
    loginMethod: LoginMethod;
    loginMethodChange: Function;
}
