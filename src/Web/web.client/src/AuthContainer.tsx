import React, { PropsWithChildren, useContext, useState } from 'react';

import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { AppRoutes, LoginMethod } from './AppRoutes';
import { msalConfig, loginRequest, teamsAppConfig } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { BearerTokenAuthProvider, createApiClient, ErrorWithCode } from '@microsoft/teamsfx';

import { BaseAxiosApiLoader, MsalAxiosApiLoader, TeamsSsoAxiosApiLoader } from './api/AxiosApiLoader';


export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [msalInitialised, setMsalInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const setLoginMethod = React.useCallback((method: LoginMethod) => { props.loginMethodChange(method) }, []);

    const teamsUserCredential = useContext(TeamsFxContext).teamsUserCredential;

    // Figure out if we can use Teams SSO or MSAL
    const initAuth = React.useCallback(() => {

        if (teamsUserCredential) {
            console.debug("We have Teams credentials. Trying to get Teams user info...");

            teamsUserCredential.getUserInfo()
                .then((info) => { console.log("Teams SSO test succesfull. User info: ", info) })
                .catch((_err: ErrorWithCode) => 
                    {
                        console.warn("Teams SSO test failed.  Falling back to MSAL");
                        setLoginMethod(LoginMethod.MSAL);
                        initMsal();
                    });

            // Hack?
            createApiClient(
                teamsAppConfig.apiEndpoint,
                new BearerTokenAuthProvider(async () => (await teamsUserCredential.getToken(loginRequest.scopes))!.token)
            );
            console.debug("Created API client with Teams SSO token");
            setApiLoader(new TeamsSsoAxiosApiLoader(teamsUserCredential, teamsAppConfig.apiEndpoint));
            setLoginMethod(LoginMethod.TeamsPopup)        
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

            const loader = new MsalAxiosApiLoader(instance, accounts[0], teamsAppConfig.apiEndpoint);
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
