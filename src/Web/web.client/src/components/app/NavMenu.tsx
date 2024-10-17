import { Component } from 'react';
import { NavLink } from 'react-router-dom';
import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';

export class NavMenu extends Component<{ apiLoader?: BaseAxiosApiLoader }> {
  static displayName = NavMenu.name;

  render() {
    return (
      <div className='nav'>
        <ul>
          <NavLink to="/tabhome">
            <li>
              <div className="menu-item"><span>Home</span></div>
            </li>
          </NavLink>

          <NavLink to="/surveyedit">
            <li>
              <div className="menu-item"><span>Survey Editor</span></div>
            </li>
          </NavLink>
          
          <NavLink to="/triggers">
            <li>
              <div className="menu-item"><span>Triggers</span></div>
            </li>
          </NavLink>

          {this.props.apiLoader &&
            <button onClick={this.props.apiLoader.logOut} style={{ backgroundColor: 'transparent', borderColor: 'transparent', width: '100%' }}>
              <li>
                <div className="menu-item"><span>Logout</span></div>
              </li>
            </button>
          }

        </ul>
      </div>
    );
  }
}
