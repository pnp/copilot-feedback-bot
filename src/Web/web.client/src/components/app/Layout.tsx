import { PropsWithChildren } from 'react';
import "./Layout.css";
import { NavMenu } from './NavMenu';
import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';

export const Layout: React.FC<PropsWithChildren<{ apiLoader?: BaseAxiosApiLoader }>> = (props) => {

  return (

    <div className="welcome page">
      <div className="narrow page-padding">
        {props.apiLoader && <NavMenu />}
        
        {props.children}

      </div>
    </div>
  );
}
