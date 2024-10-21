import React, { PropsWithChildren, useState } from 'react';

import { useMsal } from "@azure/msal-react";
import { LoginMethod } from './AppRoutes';
import { msalConfig, teamsAppConfig } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';
import { useTeamsUserCredential } from '@microsoft/teamsfx-react';

import { BaseAxiosApiLoader, MsalAxiosApiLoader, TeamsSsoAxiosApiLoader } from './api/AxiosApiLoader';

export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [msalInitialised, setMsalInitialised] = useState<boolean>(false);
    const { instance, accounts } = useMsal();

    const [loginMethod, setLoginMethod] = useState<LoginMethod | undefined>();

    const { theme, themeString, teamsUserCredential } = useTeamsUserCredential({
        initiateLoginEndpoint: teamsAppConfig.startLoginPageUrl,
        clientId: msalConfig.auth.clientId,
    });

    // Figure out if we can use Teams SSO or MSAL
    const initAuth = React.useCallback(() => {

        if (teamsUserCredential) {
            console.debug("initAuth: We have Teams credentials. Trying to get Teams user info...");

            teamsUserCredential.getUserInfo()
                .then((info) => {
                    console.log("initAuth: Teams SSO test succesfull. User info: ", info);

                    const loader = new TeamsSsoAxiosApiLoader(teamsUserCredential, teamsAppConfig.apiEndpoint);
                    setApiLoader(loader);
                    setLoginMethod(LoginMethod.TeamsSSO);

                    props.loginMethodChange(LoginMethod.TeamsSSO);
                    props.onApiLoaderReady(loader);
                })
                .catch((_err: ErrorWithCode) => {
                    console.warn("initAuth: Teams SSO test failed. Falling back to MSAL");
                    initMsal();
                });
        }
        else {
            console.debug("initAuth: No Teams credentials. Using MSAL");
            initMsal();
        }
    }, [teamsUserCredential, loginMethod]);

    React.useEffect(() => {
        initAuth();
    }, [teamsUserCredential]);

    const initMsal = React.useCallback(() => {
        setLoginMethod(LoginMethod.MSAL);
        props.loginMethodChange(LoginMethod.MSAL);

        if (!msalInitialised) {
            console.debug("initMsal: Initialising MSAL with client ID: " + msalConfig.auth.clientId);
            instance.initialize().then(() => {
                console.debug("initMsal: MSAL initialised with client ID: " + msalConfig.auth.clientId);
                setMsalInitialised(true);
            });
        }
        else {
            console.debug("initMsal: MSAL already initialised. Skipping...");
        }

    }, [msalInitialised]);


    // Init MSAL API loader if we have an account
    React.useEffect(() => {

        if (loginMethod !== LoginMethod.MSAL) {
            console.debug("Not using MSAL. Skipping MSAL API loader creation");
            return;
        }

        if (accounts.length > 0) {
            if (msalInitialised && instance && !apiLoader) {
                console.debug("Creating MSAL API loader");
                const loader = new MsalAxiosApiLoader(instance, accounts[0], teamsAppConfig.apiEndpoint);
                setApiLoader(loader);
                props.onApiLoaderReady(loader);
            }
        }
        else {
            if (msalInitialised) {
                console.info("MSAL initialised, but not authenticated yet. Will need to login to Entra ID via login page");
            }
            else
                console.warn("MSAL authentication set but not initialised yet...");
        }
    }, [msalInitialised, accounts, instance, apiLoader, loginMethod]);

    return (
        <>
            {loginMethod === undefined ?
                <div>Checking authentication method...</div>
                :
                <>
                    {loginMethod === LoginMethod.MSAL ?
                        <>
                            {msalInitialised && instance ?
                                <>
                                    {props.children}
                                </>
                                :
                                <>
                                    {instance === null ? <div>MSAL instance is null</div> :
                                        <div>Waiting for MSAL to initialise...</div>
                                    }
                                </>
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
