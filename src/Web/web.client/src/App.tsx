import React, { useState } from 'react';

import { Layout } from './components/app/Layout';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { Login } from './pages/Login/Login';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest } from "./authConfig";
import { Redirect, Route } from "react-router-dom";

export default function App() {

    const [accessToken, setAccessToken] = useState<string | null>();
    const [initialised, setInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const RequestAccessToken = React.useCallback(() => {
        const request = {
            ...loginRequest,
            account: accounts[0]
        };

        console.info(request);

        if (initialised) {
            instance.acquireTokenSilent(request).then((response) => {
                console.debug(response);
                console.debug("Got token via cached account: " + response.accessToken);
                setAccessToken(response.accessToken);
            }).catch((error) => {
                console.log(error);
                instance.acquireTokenPopup(request).then((response) => {
                    console.debug(response);
                    console.debug("Got token via popup: " + response.accessToken);
                    setAccessToken(response.accessToken);
                });
            });
        }
        else
            console.warn("Can't get access token; MSAL not initialised yet...");

    }, [accounts, instance, initialised]);

    React.useEffect(() => {

        // Get OAuth token
        if (initialised && isAuthenticated && !accessToken) {
            console.debug("Requesting access token");
            RequestAccessToken();
        }
    }, [accessToken, RequestAccessToken, isAuthenticated, initialised]);

    React.useEffect(() => {
        const initializeMsal = async () => {
            await instance.initialize();    // Initialize MSAL instance
            setInitialised(true);
        };

        initializeMsal();
    }, []);

    return (
        <div>
            {accessToken ?
                (
                    <Layout instance={instance}>
                        <AuthenticatedTemplate>
                            <Route exact path="/">
                                <Redirect to="/dashboard" />
                            </Route>
                            <Route exact path='/dashboard' render={() => <Dashboard />} />
                        </AuthenticatedTemplate>
                    </Layout>
                )
                :
                (
                    <Layout instance={instance}>
                        <UnauthenticatedTemplate>
                            <Route exact path='/' component={Login} />
                        </UnauthenticatedTemplate>
                        <AuthenticatedTemplate>
                            <p>Loading access token for API...</p>
                        </AuthenticatedTemplate>
                    </Layout>
                )}
        </div>
    );

}
