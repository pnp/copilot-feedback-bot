import React from 'react';
import { loginRequest } from "../../authConfig";
import { useMsal } from "@azure/msal-react";

export function Login() {

  const { instance } = useMsal();

  React.useEffect(() => {

    console.info(loginRequest);
    instance.loginRedirect(loginRequest)

    // eslint-disable-next-line
  }, []);

  return (
    <div>
      <span>Redirecting to sign in</span>
    </div>
  );
}
