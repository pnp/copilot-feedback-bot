import React, { useContext } from 'react';
import { loginRequest, msalConfig } from "../../authConfig";
import { Button } from '@fluentui/react-components';
import { TeamsFxContext } from '../../TeamsFxContext';
import { ErrorWithCode } from '@microsoft/teamsfx';

export const LoginPopupTeams: React.FC<{ onAuthReload: Function }> = (props) => {

  const teamsUserCredential = useContext(TeamsFxContext).teamsUserCredential;

  const login = React.useCallback(() => {
    if (!teamsUserCredential) {
      console.error("No Teams credentials available");
      return;
    }
    teamsUserCredential.login(loginRequest.scopes)
      .then(() => {
        props.onAuthReload();
      })
      .catch((error: ErrorWithCode) => {
        console.error("Failed to login for Teams SSO: " + error);
      });

  }, []);

  return (
    <div>
      <h1>Sign In to Teams</h1>
      <p>It looks like you're running this app inside of Teams but we need your consent to get access to the resources we need.</p>
      <Button className="primary" onClick={login}>Click here to Grant Consent</Button>
      <p>MSAL configuration:</p>
      <pre>{JSON.stringify(msalConfig.auth, null, 2)}</pre>

      <p>For delegated access to scopes:</p>
      <pre>{JSON.stringify(loginRequest, null, 2)}</pre>
    </div>
  );
}
