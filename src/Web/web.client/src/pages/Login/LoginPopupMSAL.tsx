import React from 'react';
import { loginRequest, msalConfig } from "../../authConfig";
import { useMsal } from "@azure/msal-react";
import { Button } from '@fluentui/react-components';

export function LoginPopupMSAL() {

  const { instance, accounts } = useMsal();
  const [error, setError] = React.useState<Error | null>(null);

  const login = React.useCallback(() => {

    console.info(loginRequest);
    instance.loginPopup(loginRequest).then(response => {
      console.info(response);
    }).catch(error => {
      setError(error);
      console.error(error);
    });

  }, []);

  const logout = React.useCallback(() => {
    instance.logoutPopup();
  }, [accounts]);

  return (
    <div>
      <h1>Sign In to Entra ID</h1>
      {accounts.length > 0 ?
        <>
          <p>Currently signed in as '{accounts[0].username}'...</p>
          <Button onClick={logout}>Logout</Button>
        </>
        :
        <>
          <p>It looks like you're running this app outside of Teams (with SSO configured).</p>
          <Button className="primary" onClick={login}>Click here to sign in with Entra ID (AAD)</Button>
          <p>MSAL configuration:</p>
          <pre>{JSON.stringify(msalConfig.auth, null, 2)}</pre>

          <p>For delegated access to scopes:</p>
          <pre>{JSON.stringify(loginRequest, null, 2)}</pre>

          {error && <p>MSAL error: {error.message}</p>}
        </>
      }

      <p>App version: dev</p>
    </div>
  );
}
