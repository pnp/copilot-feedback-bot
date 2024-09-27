import { BrowserRouter } from 'react-router-dom';
import { MsalProvider } from "@azure/msal-react";
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "./authConfig";
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import ReactDOM from 'react-dom/client'
import AppRoot from './AppRoot';

const msalInstance = new PublicClientApplication(msalConfig);

ReactDOM.createRoot(document.getElementById('root')!).render(
    <MsalProvider instance={msalInstance}>
      <BrowserRouter basename={import.meta.env.BASE_URL}>
        <LocalizationProvider dateAdapter={AdapterMoment}>
          <AppRoot />
        </LocalizationProvider>
      </BrowserRouter>
    </MsalProvider>
  );
