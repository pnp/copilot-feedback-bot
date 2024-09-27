import React, { PropsWithChildren, useState } from 'react';

import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { BaseApiLoader, MsalApiLoader } from './api/ApiLoader';
import { AppMain } from './AppMain';

export const AuthContainer : React.FC<PropsWithChildren<{onApiLoaderReady : Function}>> = (props) => {

    const [apiLoader, setApiLoader] = useState<BaseApiLoader | undefined>();
    const [initialised, setInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const InitMSAL = React.useCallback(() => {
        if (initialised) {
            setApiLoader(new MsalApiLoader(instance, accounts));
        }
        else
            console.warn("Can't get access token; MSAL not initialised yet...");

    }, [accounts, instance, initialised]);

    React.useEffect(() => {

        if (initialised && isAuthenticated && !apiLoader) {
            InitMSAL();
        }
    }, [apiLoader, InitMSAL, isAuthenticated, initialised]);

    React.useEffect(() => {
        const initializeMsal = async () => {
            await instance.initialize();    // Initialize MSAL instance
            setInitialised(true);
        };

        initializeMsal();
    }, []);

    return (
        <AppMain apiLoader={apiLoader}>{props.children}</AppMain>
    );

}
