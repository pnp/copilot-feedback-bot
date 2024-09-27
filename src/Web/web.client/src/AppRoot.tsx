
import { AuthContainer } from './AuthContainer';
import { AppMain } from './AppMain';
import { useState } from 'react';
import { BaseApiLoader } from './api/ApiLoader';

export default function AppRoot() {

    const [apiLoader, setApiLoader] = useState<BaseApiLoader | undefined>();

    return (
        <AuthContainer onApiLoaderReady={(l: BaseApiLoader) => setApiLoader(l)}>
            <AppMain apiLoader={apiLoader}>
            </AppMain>
        </AuthContainer>
    );

}
