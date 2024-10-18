
import { AuthContainer } from './AuthContainer';
import { AppRoutes, LoginMethod } from './AppRoutes';
import React, { useState } from 'react';
import { TeamsFxContext } from './TeamsFxContext';
import { msalConfig, teamsAppConfig } from './authConfig';
import { useTeamsUserCredential } from '@microsoft/teamsfx-react';
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from '@azure/msal-react';
import { BaseAxiosApiLoader } from './api/AxiosApiLoader';

export default function App() {

    const msalInstance = new PublicClientApplication(msalConfig);

    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [loginMethod, setLoginMethod] = useState<LoginMethod | undefined>();

    const [key, setKey] = React.useState(0);
    const { theme, themeString, teamsUserCredential } = useTeamsUserCredential({
        initiateLoginEndpoint: teamsAppConfig.startLoginPageUrl,
        clientId: msalConfig.auth.clientId,
    });
    const forceRerender = () => {
        setKey(key + 1);
        console.log("forceRerender: ", key);
    };

    const loginMethodChange = React.useCallback((method: LoginMethod) => {
        setLoginMethod(method);
        console.log("Login method changed to: ", method);
    }, []);
    React.useEffect(() => {
        if (teamsUserCredential) {
            setLoginMethod(LoginMethod.TeamsSSO);
        }
        else {
            setLoginMethod(LoginMethod.MSAL);
        }
    }, [teamsUserCredential]);

    React.useEffect(() => {

        let loginMethodString = "Not set";
        if (loginMethod !== undefined)
            loginMethodString = (loginMethod === LoginMethod.MSAL ? "MSAL" : "TeamsSSO");
        console.debug(`App: Login method is: ${loginMethodString}, TeamsFxContext: ${JSON.stringify(teamsUserCredential)}`);
    }, [loginMethod, teamsUserCredential]);

    return (
        <>
            {loginMethod === undefined ? <div>Loading...</div> :
                <>
                    {loginMethod === LoginMethod.MSAL ?
                        <>
                            {msalInstance ?

                                <MsalProvider instance={msalInstance}>
                                    <AuthContainer onApiLoaderReady={(l: BaseAxiosApiLoader) => setApiLoader(l)} loginMethod={loginMethod} loginMethodChange={loginMethodChange}>
                                        <AppRoutes apiLoader={apiLoader} onAuthReload={forceRerender} loginMethod={loginMethod} />
                                    </AuthContainer>
                                </MsalProvider>
                                : <div>MSAL instance not ready</div>}
                        </>
                        :
                        <TeamsFxContext.Provider value={{ theme, themeString, teamsUserCredential }}>
                            <AuthContainer onApiLoaderReady={(l: BaseAxiosApiLoader) => setApiLoader(l)} loginMethod={loginMethod} loginMethodChange={loginMethodChange}>
                                <AppRoutes apiLoader={apiLoader} onAuthReload={forceRerender} loginMethod={loginMethod} />
                            </AuthContainer>
                        </TeamsFxContext.Provider>
                    }
                </>
            }

        </>
    );
}
