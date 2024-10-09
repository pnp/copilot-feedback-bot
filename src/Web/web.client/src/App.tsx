
import { AuthContainer } from './AuthContainer';
import { AppMain } from './AppMain';
import React, { useState } from 'react';
import { BaseApiLoader } from './api/ApiLoader';
import { TeamsFxContext } from './TeamsFxContext';
import { msalConfig, teamsAppConfig } from './authConfig';
import { useTeamsUserCredential } from '@microsoft/teamsfx-react';

export default function App() {

    const [apiLoader, setApiLoader] = useState<BaseApiLoader | undefined>();

    const { theme, themeString, teamsUserCredential } = useTeamsUserCredential({
        initiateLoginEndpoint: teamsAppConfig.startLoginPageUrl,
        clientId: msalConfig.auth.clientId,
    });

    React.useEffect(() => {
        console.log("TeamsFxContext: ", teamsUserCredential);
    }, []);

    return (
        <TeamsFxContext.Provider value={{ theme, themeString, teamsUserCredential }}>
            <AuthContainer onApiLoaderReady={(l: BaseApiLoader) => setApiLoader(l)}>
                <AppMain apiLoader={apiLoader} />
            </AuthContainer>
        </TeamsFxContext.Provider>
    );

}
