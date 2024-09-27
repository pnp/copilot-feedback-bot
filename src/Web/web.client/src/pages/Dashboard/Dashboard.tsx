
import React from 'react';
import 'chartjs-adapter-date-fns'
import { BaseApiLoader } from '../../api/ApiLoader';
import { getClientConfig, ServiceConfiguration } from '../../api/ApiCalls';

export const Dashboard: React.FC<{loader: BaseApiLoader}> = (props) => {

  const [config, setConfig] = React.useState<ServiceConfiguration | null>(null);

  React.useEffect(() => {
    getClientConfig(props.loader).then((config) => {
      setConfig(config);
    });
}, []);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Home</h1>

          {config && <h2>{config.storageInfo.accountURI}</h2>}
          <p>And that.</p>
        </div>
      </section>

    </div >
  );
};
