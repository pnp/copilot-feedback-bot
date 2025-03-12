
import { AuthContainer } from './AuthContainer';
import { AppRoutes, LoginMethod } from './AppRoutes';
import React, { useState } from 'react';

import { BaseAxiosApiLoader } from './api/AxiosApiLoader';
import { teamsLightTheme, Theme } from '@fluentui/react-components';

export const App: React.FC <{}> = () => {

    const [theme, setTheme] = useState<Theme>(teamsLightTheme);
    const [apiLoader, setApiLoader] = useState<BaseAxiosApiLoader | undefined>();
    const [loginMethod, setLoginMethod] = useState<LoginMethod | undefined>();

    const [key, setKey] = React.useState(0);

    const forceRerender = () => {
        setKey(key + 1);
        console.log("forceRerender: ", key);
    };

    const loginMethodChange = React.useCallback((method: LoginMethod) => {
        setLoginMethod(method);
        console.log("Login method changed to: ", method);
    }, []);

    return (
        <>
            <AuthContainer teamsThemeChange={(style : Theme) => setTheme(style)} onApiLoaderReady={(l: BaseAxiosApiLoader) => setApiLoader(l)} loginMethodChange={loginMethodChange}>
                <AppRoutes theme={theme} apiLoader={apiLoader} onAuthReload={forceRerender} loginMethod={loginMethod} />
            </AuthContainer>
        </>
    );
}
