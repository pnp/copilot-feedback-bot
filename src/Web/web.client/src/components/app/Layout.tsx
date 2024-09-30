import { PropsWithChildren } from 'react';
import { BaseApiLoader } from '../../api/ApiLoader';
import "./Layout.css";
import { NavMenu } from './NavMenu';

export const Layout: React.FC<PropsWithChildren<{ apiLoader?: BaseApiLoader }>> = (props) => {

  return (

    <div className="welcome page">
      <div className="narrow page-padding">
        <NavMenu />
        {props.children}

      </div>
    </div>
  );
}
