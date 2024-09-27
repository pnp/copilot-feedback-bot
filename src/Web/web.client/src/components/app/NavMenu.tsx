import { Component } from 'react';
import { IPublicClientApplication } from '@azure/msal-browser';
import { NavLink } from 'react-router-dom';

export class NavMenu extends Component<{ profile: LoginProfile | null, instance: IPublicClientApplication }> {
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

        {this.props.profile &&
          <>
            <NavLink to="/skills">
              <li className="analytics">
                <div className="menu-item"><span>Skills Value</span></div>
              </li>
            </NavLink>
            <NavLink to="/skillsflow">
              <li className="skillsflow">
                <div className="menu-item"><span>Skills In vs Out</span></div>
              </li>
            </NavLink>
            <NavLink to="/initiatives">
              <li className="initiatives">
                <div className={`menu-item`}><span>Initiatives</span></div>
              </li>
            </NavLink>

            <NavLink to="/DataQuality">
              <li className="quality">
                <div className="menu-item"><span>Data Quality</span></div>
              </li>
            </NavLink>

            <NavLink to="/wiki">
              <li className="wiki">
                <div className="menu-item"><span>Skills Wiki</span></div>
              </li>
            </NavLink>
            <NavLink to="/importLogs">
              <li className="imports">
                <div className="menu-item"><span>Imports</span></div>
              </li>
            </NavLink>
            <NavLink to="/loginsList">
              <li className="profile">
                <div className="menu-item"><span>Users</span></div>
              </li>
            </NavLink>
          </>
        }

        <button onClick={() => this.logout(this.props.instance)} style={{ backgroundColor: 'transparent', borderColor: 'transparent', width: '100%' }}>
          <li className="logout">
            <div className="menu-item"><span>Logout</span></div>
          </li>
        </button>
      </ul>
    );
  }
}
