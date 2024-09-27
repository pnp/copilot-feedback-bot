import React, { useState } from 'react';

import { Layout } from './components/app/Layout';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { LoginRedirect } from './pages/Login/LoginRedirect';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { Redirect, Route } from "react-router-dom";
import { BaseApiLoader, MsalApiLoader } from './api/ApiLoader';

export default function App() {

    const [apiLoader, setApiLoader] = useState<BaseApiLoader | null>();
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

        // Get OAuth token
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
        <div>
            {apiLoader ?
                (
                    <Layout instance={instance}>
                        <AuthenticatedTemplate>
                            <Route exact path="/">
                                <Redirect to="/dashboard" />
                            </Route>
                            <Route exact path='/dashboard' render={() => <Dashboard loader={apiLoader} />} />
                        </AuthenticatedTemplate>
                    </Layout>
                )
                :
                (
                    <Layout instance={instance}>
                        <UnauthenticatedTemplate>
                            <Route exact path='/' component={LoginRedirect} />
                        </UnauthenticatedTemplate>
                        <AuthenticatedTemplate>
                            <p>Loading access token for API...</p>
                        </AuthenticatedTemplate>
                    </Layout>
                )}
        </div>
    );

}
