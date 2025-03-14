import React, { PropsWithChildren, useState } from 'react';

import { useMsal } from "@azure/msal-react";
import { LoginMethod } from './AppRoutes';
import { msalConfig, teamsAppConfig } from "./authConfig";
import { TeamsFxContext } from './TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';
import { useTeamsUserCredential } from '@microsoft/teamsfx-react';

import { BaseAxiosApiLoader, MsalAxiosApiLoader, TeamsSsoAxiosApiLoader } from './api/AxiosApiLoader';
import { getBasicStats } from './api/ApiCalls';
import { getRootUrl } from './utils/DataUtils';
import { teamsDarkTheme, teamsHighContrastTheme, teamsLightTheme, Theme } from '@fluentui/react-components';

export const AuthContainer: React.FC<PropsWithChildren<AuthContainerProps>> = (props) => {

    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [error, setError] = useState<string | undefined>()
    const [loading, setLoading] = useState<boolean>(false);
    const [msalInitialised, setMsalInitialised] = useState<boolean>(false);
    const { instance, accounts } = useMsal();

    const [loginMethod, setLoginMethod] = useState<LoginMethod | undefined>();

    const { theme, themeString, teamsUserCredential } = useTeamsUserCredential({
        initiateLoginEndpoint: teamsAppConfig.startLoginPageUrl,
        clientId: msalConfig.auth.clientId,
    });

    React.useEffect(() => {
        const style: Theme = themeString === "dark"
            ? teamsDarkTheme
            : themeString === "contrast"
                ? teamsHighContrastTheme
                : {
                    ...teamsLightTheme,
                    colorNeutralBackground3: "#eeeeee",
                };
        console.debug("Theme change:")
        console.debug(style);
        props.teamsThemeChange(style);
    }, [themeString]);

    // Figure out if we can use Teams SSO or MSAL
    const initAuth = React.useCallback(() => {

        if (teamsUserCredential) {
            console.debug("initAuth: We have Teams credentials. Trying to get Teams user info...");

            teamsUserCredential.getUserInfo()
                .then((info) => {
                    console.log("initAuth: Teams SSO test succesfull. User info: ", info);

                    const loader = new TeamsSsoAxiosApiLoader(teamsUserCredential, getRootUrl(window.location.href));
                    setApiLoader(loader);
                    setLoginMethod(LoginMethod.TeamsSSO);

                    props.loginMethodChange(LoginMethod.TeamsSSO);

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
                const loader = new MsalAxiosApiLoader(instance, accounts[0], getRootUrl(window.location.href));
                setApiLoader(loader);
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

    // Test API connection and raise event if succesfull
    React.useEffect(() => {
        if (apiLoader) {
            setLoading(true);
            console.debug("Testing API connection...");
            getBasicStats(apiLoader)
                .then((response) => {
                    console.log("API test response: ", response);
                    setError(undefined);
                    props.onApiLoaderReady(apiLoader);
                    setLoading(false);
                })
                .catch((err) => {
                    console.error("API test failed: ", err);
                    setError(err.toString());
                    setLoading(false);
                });
        }
    }, [apiLoader]);

    return (
        <>
            {error ?
                <div>API test error: {error}. Please check authentication configuration, server-side and JavaScript. Refresh page to retry.</div>
                : null}

            {loading ?
                <div>
                    Checking back-end API connectivity...
                </div> :
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
            }


        </>
    );
}


export interface AuthContainerProps {
    onApiLoaderReady: Function;
    loginMethodChange: Function;
    teamsThemeChange: Function;
}
