import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { SelectTabData, SelectTabEvent, Spinner, Tab, TabList, TabValue } from '@fluentui/react-components';
import React, { useState } from 'react';
import { getUsageStatsReport } from '../../api/ApiCalls';

import { ChartContainer } from '../../components/app/ChartContainer';
import { UsageStatsReport } from '../../apimodels/Models';
import { LookupLeagueChart } from './LookupLeagueChart';
import { UsersLeagueChart } from './UsersLeagueChart';

export function OnboardingHome(props: { loader?: BaseAxiosApiLoader }) {
  const [error, setError] = React.useState<string | null>(null);
  const [stats, setStats] = React.useState<UsageStatsReport | null>(null);

  const [selectedValue, setSelectedValue] = useState<TabValue>("users");
  const onTabSelect = (_: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
  };

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
      <p>Your highest engaged users in O365 are here. By individual users, or by demographics.</p>
      {error ? <p className='error'>{error}</p> :
        <>
          {stats ?
            <div>
              <ChartContainer>
                <div style={{ display: 'flex' }}>
                  <div style={{ minWidth: '200px' }}>
                    <TabList selectedValue={selectedValue} onTabSelect={onTabSelect} vertical>
                      <Tab id="users" value="users">
                        Users League
                      </Tab>
                      <Tab id="departments" value="departments">
                        Departments League
                      </Tab>
                      <Tab id="countries" value="countries">
                        Countries League
                      </Tab>
                    </TabList>
                  </div>
                  <div>
                    {selectedValue === "users" && (
                      <div>
                        <UsersLeagueChart data={stats.usersLeague} />
                      </div>
                    )}
                    {selectedValue === "countries" && (
                      <div>
                        <LookupLeagueChart propName="Countries League" data={stats.countriesLeague} />
                      </div>
                    )}
                    {selectedValue === "departments" && (
                      <div>
                        <LookupLeagueChart propName="Departments League" data={stats.departmentsLeague} />
                      </div>
                    )}
                  </div>
                </div>

              </ChartContainer>
            </div>
            :
            <Spinner />
          }
        </>
      }
    </div>
  );
}
