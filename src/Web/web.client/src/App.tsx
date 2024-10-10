
import { AuthContainer } from './AuthContainer';
import { AppRoutes, LoginMethod } from './AppRoutes';
import React, { useState } from 'react';
import { BaseApiLoader } from './api/ApiLoader';
import { TeamsFxContext } from './TeamsFxContext';
import { msalConfig, teamsAppConfig } from './authConfig';
import { useTeamsUserCredential } from '@microsoft/teamsfx-react';
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from '@azure/msal-react';

export default function App() {

    const msalInstance = new PublicClientApplication(msalConfig);

    const [apiLoader, setApiLoader] = useState<BaseApiLoader | undefined>();
    const [loginMethod, setLoginMethod] = useState<LoginMethod>(LoginMethod.MSAL);

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
            setLoginMethod(LoginMethod.TeamsPopup);
        }
        else {
            setLoginMethod(LoginMethod.MSAL);
        }
    }, [teamsUserCredential]);

    React.useEffect(() => {
        console.log("Login method is: ", loginMethod);
        console.log("TeamsFxContext: ", teamsUserCredential);
        teamsUserCredential?.getUserInfo().then((info) => { console.log("User info: ", info) });
    }, [loginMethod]);

    return (
        <>
            {loginMethod === LoginMethod.MSAL ?
                <MsalProvider instance={msalInstance}>
                    <AuthContainer onApiLoaderReady={(l: BaseApiLoader) => setApiLoader(l)} loginMethod={loginMethod} loginMethodChange={loginMethodChange}>
                        <AppRoutes apiLoader={apiLoader} onAuthReload={forceRerender} loginMethod={loginMethod} />
                    </AuthContainer>
                </MsalProvider>
                :
                <TeamsFxContext.Provider value={{ theme, themeString, teamsUserCredential }}>
                    <AuthContainer onApiLoaderReady={(l: BaseApiLoader) => setApiLoader(l)} loginMethod={loginMethod} loginMethodChange={loginMethodChange}>
                        <AppRoutes apiLoader={apiLoader} onAuthReload={forceRerender} loginMethod={loginMethod} />
                    </AuthContainer>
                </TeamsFxContext.Provider>
            }
        </>
    );
}
