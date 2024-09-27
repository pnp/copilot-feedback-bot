import { IPublicClientApplication } from '@azure/msal-browser';
import { NavMenu } from './NavMenu';
import { NavMenuProfile } from '../common/controls/NavMenuProfile';
import { PropsWithChildren } from 'react';

export const Layout: React.FC<PropsWithChildren<{ instance: IPublicClientApplication }>> = (props) => {

  return (

    <div id="page-container" className="page-container">

      <nav className="pushy pushy-left">
        <a href="/" className="pushy-logo">
          <div className="logo">
            <img className="companyLogo" src="img/sc-logo.png" alt="Skill Cap"></img>
          </div>
        </a>

        <NavMenu instance={props.instance} />
        <NavMenuProfile />
        <p className="site-overlay">
          <img className="close-btn" src="img/close.svg" alt="close" height="24" width="24" />
        </p>
      </nav>
      <div className="site-overlay"></div>

      <main className="wrapper dashboard">
        <div className="nav-wrapper">
          <div className="nav-wrap">
            <button className="menu-btn">
              <img src="img/menu-icon.svg" alt="menu" />
            </button>

            <a href="/">
              <div className="logo">
                <img className="companyLogo" src="img/sc-logo.png" alt="Skill Cap"></img>
              </div>
            </a>

            <div className="nav-links">
              <nav className="main-nav">
                <NavMenu instance={props.instance} />
              </nav>
            </div>

            <NavMenuProfile />

          </div>
        </div>

        <div className="portal-body">
          <div className="container full">
            {props.children}
          </div>
        </div>
      </main>

      <footer></footer>
    </div>
  );
}