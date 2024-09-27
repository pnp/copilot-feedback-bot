import React, { useState } from 'react';

import { Layout } from './components/app/Layout';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { Login } from './pages/Login/Login';
import { LoginsList } from './pages/LoginsList/LoginsList';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest } from "./authConfig";
import { SkillsInsights } from './pages/SkillsInsights/SkillsInsightsPage';
import { Imports } from './pages/Imports/Imports';
import { LoginsInvite } from './pages/LoginsInvite/LoginsInvite';
import { LoginsInviteFailed } from './pages/LoginsInvite/LoginsInviteFailed';
import { SkillsFlowHome } from './pages/SkillsInVsOut/SkillsFlowHome';
import { InitiativesPage } from './pages/Initiatives/InitiativesPage';
import { DataQualityDashboard } from './pages/DataQuality/DataQualityDashboard';
import { Redirect, Route } from "react-router-dom";
import { WikiHome } from './pages/Wiki/WikiHome';

declare global {
    interface Window {
        initMainJs: () => void;
        initCharts: (overallChange: number, monthChange: number, sevenDayChange: number) => void;
    }
}

export default function App() {

    const [accessToken, setAccessToken] = useState<string | null>();
    const [initialised, setInitialised] = useState<boolean>(false);
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const [loginProfile, setLoginProfile] = React.useState<LoginProfile | null>();

    const RequestAccessToken = React.useCallback(() => {
        const request = {
            ...loginRequest,
            account: accounts[0]
        };

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

        window.initMainJs();
    }, []);

    return (
        <div>
            {accessToken ?
                (
                    <Layout profile={loginProfile!} instance={instance}>
                        <AuthenticatedTemplate>
                            <Route exact path="/">
                                <Redirect to="/dashboard" />
                            </Route>
                            <Route exact path='/dashboard' render={() => <Dashboard {... { token: accessToken! }} profileLoaded={(p: LoginProfile) => setLoginProfile(p)} />} />
                            <Route exact path='/inviteFailed' render={() => <LoginsInviteFailed />} />

                            {loginProfile &&
                                <>
                                    <Route exact path='/skills' render={() => <SkillsInsights {... { token: accessToken! }} client={loginProfile.client} />} />
                                    <Route exact path='/skillsflow' render={() => <SkillsFlowHome {... { token: accessToken! }} client={loginProfile.client} />} />
                                    <Route exact path='/initiatives' render={() => <InitiativesPage {... { token: accessToken! }} client={loginProfile.client} />} />
                                    <Route exact path='/importLogs' render={() => <Imports {... { token: accessToken! }} client={loginProfile.client} />} />
                                    <Route exact path='/loginsList' render={() => <LoginsList {... { token: accessToken! }} client={loginProfile.client} />} />
                                    <Route exact path='/loginsInvite' render={() => <LoginsInvite {... { token: accessToken! }} client={loginProfile.client} />} />
                                    <Route exact path='/DataQuality' render={() => <DataQualityDashboard {... { token: accessToken! }} />} />
                                    <Route exact path='/wiki' render={() => <WikiHome {... { token: accessToken! }} />} />
                                </>
                            }
                        </AuthenticatedTemplate>
                        <UnauthenticatedTemplate>
                            <Route exact path='/' component={Login} />
                        </UnauthenticatedTemplate>
                    </Layout>
                )
                :
                (
                    <Layout profile={null} instance={instance}>
                        <UnauthenticatedTemplate>
                            <Route exact path='/' component={Login} />
                            <Route exact path='/inviteFailed' render={() => <LoginsInviteFailed />} />
                        </UnauthenticatedTemplate>
                        <AuthenticatedTemplate>
                            <p>Loading access token for API...</p>
                        </AuthenticatedTemplate>
                    </Layout>
                )}
        </div>
    );

}
