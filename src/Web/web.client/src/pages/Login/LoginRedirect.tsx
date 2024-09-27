import React from 'react';
import { loginRequest } from "../../authConfig";
import { useMsal } from "@azure/msal-react";

export function LoginRedirect() {

  const { instance } = useMsal();

  React.useEffect(() => {

    console.info(loginRequest);
    instance.loginRedirect(loginRequest)

  }, []);

  return (
    <div>
      <span>Redirecting to sign in</span>
    </div>
  );
}
