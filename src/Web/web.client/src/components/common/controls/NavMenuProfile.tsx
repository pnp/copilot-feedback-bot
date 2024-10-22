import { IMsalContext, useMsal } from "@azure/msal-react";
import { IdTokenClaims } from "../../../apimodels/Models";

export const NavMenuProfile: React.FC<{ }> = () => {
  const userCtx: IMsalContext = useMsal();
  const loginClaims = (userCtx && userCtx.accounts[0] ? userCtx.accounts[0].idTokenClaims : null) as IdTokenClaims | null;

  return (
    <div className="user-nav">
      <div className="avatar">
        <img src="img/avatar.png" alt="avatar" />
      </div>
      <div className="user-txt">
        <p><strong>{loginClaims?.given_name} {loginClaims?.family_name}</strong></p>
        <p className="role">Contoso Inc</p>
      </div>
    </div>
  );
}
