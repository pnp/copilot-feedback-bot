import { Component } from 'react';
import { NavLink } from 'react-router-dom';
import { BaseApiLoader } from '../../api/ApiLoader';

export class NavMenu extends Component<{ apiLoader?: BaseApiLoader }> {
  static displayName = NavMenu.name;

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


        {this.props.apiLoader &&

          <button onClick={this.props.apiLoader.logOut} style={{ backgroundColor: 'transparent', borderColor: 'transparent', width: '100%' }}>
            <li className="logout">
              <div className="menu-item"><span>Logout</span></div>
            </li>
          </button>
        }

      </ul>
    );
  }
}
