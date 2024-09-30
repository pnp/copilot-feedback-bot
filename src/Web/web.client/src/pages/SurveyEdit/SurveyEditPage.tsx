
import React from 'react';
import 'chartjs-adapter-date-fns'
import { BaseApiLoader } from '../../api/ApiLoader';
import { getClientConfig, ServiceConfiguration } from '../../api/ApiCalls';

export const Dashboard: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const [config, setConfig] = React.useState<ServiceConfiguration | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getClientConfig(props.loader).then((config) => {
        setConfig(config);
      });
  }, [props.loader]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Welcome to the copilot feedback bot control panel app.</p>
        </div>
      </section>

    </div >
  );
};
