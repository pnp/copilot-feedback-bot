import React, { PropsWithChildren } from 'react';

import { Layout } from './components/app/Layout';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { LoginRedirect } from './pages/Login/LoginRedirect';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";
import { Redirect, Route } from "react-router-dom";
import { BaseApiLoader } from './api/ApiLoader';

export const AppMain : React.FC<PropsWithChildren<{apiLoader?: BaseApiLoader}>> = (props) => {

    return (
        <div>
            {props.apiLoader ?
                (
                    <Layout apiLoader={props.apiLoader}>
                        <AuthenticatedTemplate>
                            <Route exact path="/">
                                <Redirect to="/dashboard" />
                            </Route>
                            <Route exact path='/dashboard' render={() => <Dashboard loader={props.apiLoader} />} />
                        </AuthenticatedTemplate>
                    </Layout>
                )
                :
                (
                    <Layout apiLoader={props.apiLoader}>
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
