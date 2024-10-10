import { BrowserRouter } from 'react-router-dom';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import ReactDOM from 'react-dom/client'
import App from './App';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <BrowserRouter basename={import.meta.env.BASE_URL}>
    <LocalizationProvider dateAdapter={AdapterMoment}>
      <App />
    </LocalizationProvider>
  </BrowserRouter>
);
