import React, { PropsWithChildren } from 'react';

import { Layout } from './components/app/Layout';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { LoginPopupMSAL } from './pages/Login/LoginPopupMSAL';
import { Redirect, Route } from "react-router-dom";
import { SurveyManagerPage } from './pages/SurveyEdit/SurveyManagerPage';
import { FluentProvider, teamsLightTheme } from '@fluentui/react-components';
import { TriggersPage } from './pages/Triggers/TriggersPage';
import { LoginPopupTeams } from './pages/Login/LoginPopupTeams';
import { BaseAxiosApiLoader } from './api/AxiosApiLoader';

export const AppRoutes: React.FC<PropsWithChildren<AppRoutesProps>> = (props) => {

    return (
        <FluentProvider theme={teamsLightTheme}>
            {props.apiLoader ?
                (
                    <Layout apiLoader={props.apiLoader}>
                        <Route exact path="/">
                            <Redirect to="/tabhome" />
                        </Route>
                        <Route exact path='/tabhome' render={() => <Dashboard loader={props.apiLoader} />} />
                        <Route exact path='/surveyedit' render={() => <SurveyManagerPage loader={props.apiLoader} />} />
                        <Route exact path='/triggers' render={() => <TriggersPage loader={props.apiLoader} />} />
                    </Layout>
                )
                :
                (
                    <Layout>
                        <Route exact path="/">
                            {props.loginMethod === LoginMethod.MSAL &&
                                <LoginPopupMSAL />
                            }
                            {props.loginMethod === LoginMethod.TeamsSSO &&
                                <LoginPopupTeams onAuthReload={props.onAuthReload} />
                            }
                        </Route>
                    </Layout>
                )}
        </FluentProvider>
    );
}
interface AppRoutesProps { apiLoader?: BaseAxiosApiLoader, loginMethod?: LoginMethod, onAuthReload: Function }

export enum LoginMethod {
    MSAL,
    TeamsSSO
}
