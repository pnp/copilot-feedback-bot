import React, { PropsWithChildren, useState } from 'react';

import { MsalProvider, useIsAuthenticated } from "@azure/msal-react";
import { LoginMethod } from './AppRoutes';
import { msalConfig, teamsAppConfig } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';
import { PublicClientApplication } from "@azure/msal-browser";
import { useTeamsUserCredential } from '@microsoft/teamsfx-react';

import { BaseAxiosApiLoader, MsalAxiosApiLoader, TeamsSsoAxiosApiLoader } from './api/AxiosApiLoader';

export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    let msalInstance : PublicClientApplication | null = null;

    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [msalInitialised, setMsalInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();

    const [loginMethod, setLoginMethod] = useState<LoginMethod | undefined>();

    const { theme, themeString, teamsUserCredential } = useTeamsUserCredential({
        initiateLoginEndpoint: teamsAppConfig.startLoginPageUrl,
        clientId: msalConfig.auth.clientId,
    });

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
                    initMsal();
                });
        }
        else    
        {
            console.debug("No Teams credentials. Using MSAL");
            initMsal();
        }
    }, [teamsUserCredential]);

    
    React.useEffect(() => {
        if (teamsUserCredential) {
            setLoginMethod(LoginMethod.TeamsSSO);
        }
        else {
            setLoginMethod(LoginMethod.MSAL);
        }
    }, [teamsUserCredential]);

    // Debug out login method
    React.useEffect(() => {

        let loginMethodString = "Not set";
        if (loginMethod !== undefined)
            loginMethodString = (loginMethod === LoginMethod.MSAL ? "MSAL" : "TeamsSSO");
        console.debug(`App: Login method is: ${loginMethodString}, TeamsFxContext: ${JSON.stringify(teamsUserCredential)}`);
    }, [loginMethod, teamsUserCredential]);



    const initMsal = React.useCallback(() => {
        setLoginMethod(LoginMethod.MSAL);
        if (!msalInstance) msalInstance = new PublicClientApplication(msalConfig);
         
        if (!msalInitialised) {
            msalInstance.initialize().then(() => {
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

        if (loginMethod !== LoginMethod.MSAL) {
            console.debug("Not using MSAL. Skipping MSAL API loader creation");
            return;
        }
        if (msalInitialised && isAuthenticated && msalInstance && !apiLoader) {
            console.debug("Creating MSAL API loader");
            const accounts = msalInstance?.getAllAccounts();
            const loader = new MsalAxiosApiLoader(msalInstance, accounts[0], teamsAppConfig.apiEndpoint);
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
            {loginMethod === undefined ?
                <div>Checking authentication method...</div>
                :
                <>
                    {loginMethod === LoginMethod.MSAL ?
                        <>
                            {msalInitialised && msalInstance ?
                                <MsalProvider instance={msalInstance}>
                                    <p>MSAL initialised</p>
                                    {props.children}
                                </MsalProvider>
                                :
                                <div>Waiting for MSAL to initialise...</div>
                            }
                        </>
                        :
                        <>
                            {loginMethod === LoginMethod.TeamsSSO ?
                                <TeamsFxContext.Provider value={{ theme, themeString, teamsUserCredential }}>
                                    {props.children}
                                </TeamsFxContext.Provider>
                                :
                                <div>Error: Unknown authentication type</div>
                            }
                        </>
                    }
                </>
            }
        </>
    );
}


export interface AuthContainerProps {
    onApiLoaderReady: Function;
    loginMethodChange: Function;
}
