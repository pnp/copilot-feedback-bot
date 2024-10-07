import React from 'react';
import { loginRequest } from "../../authConfig";
import { useMsal } from "@azure/msal-react";
import { Button } from '@fluentui/react-components';

export function LoginRedirect() {

  const { instance } = useMsal();

  const login = React.useCallback(() => {

    console.info(loginRequest);
    instance.loginRedirect(loginRequest)

  }, []);

  return (
    <div>
      <h1>Sign In</h1>
      <p>It looks like you're running this app outside of Teams (with SSO configured)</p>
      <Button className="primary" onClick={login}>Click here to sign in with Entra ID (AAD)</Button>
    </div>
  );
}
