import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { Spinner } from '@fluentui/react-components';
import React from 'react';
import { getUsageStatsReport } from '../../api/ApiCalls';

import { ChartContainer } from '../../components/app/ChartContainer';
import { UsageStatsReport } from '../../apimodels/Models';
import { LookupLeagueChart } from './LookupLeagueChart';

export function OnboardingHome(props: { loader?: BaseAxiosApiLoader }) {
  const [error, setError] = React.useState<string | null>(null);
  const [stats, setStats] = React.useState<UsageStatsReport | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getUsageStatsReport(props.loader).then((d) => {
        setStats(d);
      }).catch((e: Error) => {
        console.error("Error: ", e);
        setError(e.toString());
      });
  }, [props.loader]);

  return (
    <div>
      <h1>Onboarding</h1>
      {error ? <p className='error'>{error}</p> :
        <>
          {stats ?
            <div>
              <ChartContainer>
                <LookupLeagueChart propName="Countries League" data={stats.countriesLeague} />
              </ChartContainer>
            </div> :
            <Spinner />
          }
        </>
      }
    </div>
  );
}
