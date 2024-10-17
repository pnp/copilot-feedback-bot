import React, { PropsWithChildren, useContext, useState } from 'react';

import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { AppRoutes, LoginMethod } from './AppRoutes';
import { msalConfig, teamsAppConfig } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';

import { BaseAxiosApiLoader, MsalAxiosApiLoader } from './api/AxiosApiLoader';
import { useData } from '@microsoft/teamsfx-react';
import { getBasicStats } from './api/ApiCalls';


export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [msalInitialised, setMsalInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const setLoginMethod = React.useCallback((method: LoginMethod) => { props.loginMethodChange(method) }, []);
    const [needConsent, setNeedConsent] = useState(false);

    const teamsUserCredential = useContext(TeamsFxContext).teamsUserCredential;

    const { reload } = useData(async () => {
        if (!teamsUserCredential) {
            throw new Error("TeamsFx SDK is not initialized.");
        }
        if (needConsent) {
            await teamsUserCredential!.login(["User.Read"]);
            setNeedConsent(false);
        }
        try {
            console.log("Getting basic stats...");
            const functionRes = await getBasicStats(apiLoader!);
            console.log("Got basic stats: ", functionRes);
            return functionRes;
        } catch (error: any) {
            if (error.message.includes("The application may not be authorized.")) {
                setNeedConsent(true);
            }
        }
    });

    // Figure out if we can use Teams SSO or MSAL
    const initAuth = React.useCallback(() => {

        if (teamsUserCredential) {
            console.debug("We have Teams credentials. Trying to get Teams user info...");

            teamsUserCredential.getUserInfo()
                .then((info) => {
                    console.log("Teams SSO test succesfull. User info: ", info);
                    reload();
                })
                .catch((_err: ErrorWithCode) => {
                    console.warn("Teams SSO test failed.  Falling back to MSAL");
                    setLoginMethod(LoginMethod.MSAL);
                    initMsal();
                });

            // Hack?
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

            if (!msalInitialised) {
                await instance.initialize();    // Initialize MSAL instance
                setMsalInitialised(true);
                console.debug("MSAL initialised with client ID: " + msalConfig.auth.clientId);
            }
            else {
                console.warn("MSAL already initialised. Skipping...");
            }
        };

        initializeMsal();
    }, [apiLoader, msalInitialised]);

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
    loginMethod?: LoginMethod;
    loginMethodChange: Function;
}
