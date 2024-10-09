import React, { PropsWithChildren, useContext, useState } from 'react';

import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { BaseApiLoader, MsalApiLoader, TeamsSsoApiLoader } from './api/ApiLoader';
import { AppRoutes, LoginMethod } from './AppRoutes';
import { msalConfig, loginRequest } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';

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
            
            teamsUserCredential.getToken(loginRequest.scopes).then(() => {
                console.debug("Teams SSO worked. Using Teams SSO API loader");
                const loader = new TeamsSsoApiLoader(teamsUserCredential);
                setApiLoader(loader);
                props.onApiLoaderReady(loader);
            }).catch((error: ErrorWithCode) => {
                if (error.code === "UiRequiredError") {
                    console.warn("User login required via popup");
                    setLoginMethod(LoginMethod.TeamsPopup); 
                }
                else {
                    console.error("Failed to get token from Teams SSO: " + error);
                    initMsal();
                }
            });
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
