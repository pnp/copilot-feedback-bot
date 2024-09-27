import { Component } from 'react';
import { IPublicClientApplication } from '@azure/msal-browser';
import { NavLink } from 'react-router-dom';

export class NavMenu extends Component<{ instance: IPublicClientApplication }> {
  static displayName = NavMenu.name;

  logout(instance: IPublicClientApplication) {
    instance.logoutRedirect({ postLogoutRedirectUri: "/" })
  }

  render() {
    return (
      <ul className="menu">
        <NavLink to="/dashboard">
          <li className="dashboard">
            <div className="menu-item"><span>Dashboard</span></div>
          </li>
        </NavLink>

        <NavLink to="/skills">
          <li className="analytics">
            <div className="menu-item"><span>Skills Value</span></div>
          </li>
        </NavLink>

        <button onClick={() => this.logout(this.props.instance)} style={{ backgroundColor: 'transparent', borderColor: 'transparent', width: '100%' }}>
          <li className="logout">
            <div className="menu-item"><span>Logout</span></div>
          </li>
        </button>
      </ul>
    );
  }
}
