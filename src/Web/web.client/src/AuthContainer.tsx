import React, { PropsWithChildren, useContext, useState } from 'react';

import { MsalProvider, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { LoginMethod } from './AppRoutes';
import { msalConfig, teamsAppConfig } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';
import { PublicClientApplication } from "@azure/msal-browser";

import { BaseAxiosApiLoader, MsalAxiosApiLoader, TeamsSsoAxiosApiLoader } from './api/AxiosApiLoader';

export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    const msalInstance = new PublicClientApplication(msalConfig);

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
                .then((info) => {
                    console.log("Teams SSO test succesfull. User info: ", info);

                    const loader = new TeamsSsoAxiosApiLoader(teamsUserCredential, teamsAppConfig.apiEndpoint);
                    setApiLoader(loader);
                    setLoginMethod(LoginMethod.TeamsSSO);
                    props.onApiLoaderReady(loader);
                })
                .catch((_err: ErrorWithCode) => {
                    console.warn("Teams SSO test failed. Falling back to MSAL");
                    setLoginMethod(LoginMethod.MSAL);
                    initMsal();
                });
        }
    }, [teamsUserCredential]);


    const initMsal = React.useCallback(() => {

        if (!msalInitialised) {
            instance.initialize().then(() => {
                console.debug("MSAL initialised with client ID: " + msalConfig.auth.clientId);
                setMsalInitialised(true);
            });
        }
        else {
            console.warn("MSAL already initialised. Skipping...");
        }

    }, [apiLoader, msalInitialised]);


    React.useEffect(() => {
        initAuth();
    }, [teamsUserCredential, initAuth]);

    // Init MSAL API loader if we have an account
    React.useEffect(() => {

        if (props.loginMethod !== LoginMethod.MSAL) {
            console.debug("Not using MSAL. Skipping MSAL API loader creation");
            return;
        }
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
                console.warn("MSAL authentication set but not initialised yet...");
        }
    }, [msalInitialised, isAuthenticated]);

    return (
        <>
        {props.loginMethod

        }
            {msalInitialised &&
                <MsalProvider instance={msalInstance}>
                    {props.children}
                </MsalProvider>
            }
            {props.children}</>
    );
}


export interface AuthContainerProps {
    onApiLoaderReady: Function;
    loginMethod?: LoginMethod;
    loginMethodChange: Function;
}
