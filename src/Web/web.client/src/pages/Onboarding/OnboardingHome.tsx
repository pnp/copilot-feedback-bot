import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { Button, Caption1, Card, CardHeader, Spinner } from '@fluentui/react-components';
import React from 'react';
import { getUsageStatsReport } from '../../api/ApiCalls';
import { SurveyStatsChart } from '../Home/SurveyStatsChart';
import { UserStatsChart } from '../Home/UserStatsChart';

import { ChartContainer } from '../../components/app/ChartContainer';
import { UsageStatsReport } from '../../apimodels/Models';
import { useStyles } from '../../utils/styles';

export function OnboardingHome(props: { loader?: BaseAxiosApiLoader }) {
  const [error, setError] = React.useState<string | null>(null);
  const [basicStats, setBasicStats] = React.useState<UsageStatsReport | null>(null);
  const styles = useStyles();

  React.useEffect(() => {
    if (props.loader)
      getUsageStatsReport(props.loader).then((d) => {
        setBasicStats(d);
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
          {basicStats ?
            <div>
              <ChartContainer>

              </ChartContainer>
            </div> :
            <Spinner />
          }
        </>
      }
    </div>
  );
}
