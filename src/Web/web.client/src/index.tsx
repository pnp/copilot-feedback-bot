import { BrowserRouter } from 'react-router-dom';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import ReactDOM from 'react-dom/client'
import { App } from './App';
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from './authConfig';
import { MsalProvider } from '@azure/msal-react';

const msalInstance = new PublicClientApplication(msalConfig);


ReactDOM.createRoot(document.getElementById('root')!).render(
  <BrowserRouter basename={import.meta.env.BASE_URL}>
    <LocalizationProvider dateAdapter={AdapterMoment}>
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    </LocalizationProvider>
  </BrowserRouter>
);
