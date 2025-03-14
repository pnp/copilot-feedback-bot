
import React from 'react';
import 'chartjs-adapter-date-fns'
import { getBasicStats } from '../../api/ApiCalls';
import { BasicStats } from '../../apimodels/Models';
import { Button, Caption1, Card, CardHeader, Spinner, Text } from '@fluentui/react-components';
import { SurveyStatsChart } from './SurveyStatsChart';
import { ChartContainer } from '../../components/app/ChartContainer';
import { UserStatsChart } from './UserStatsChart';
import { MoreHorizontal20Regular } from "@fluentui/react-icons";
import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { useStyles } from '../../utils/styles';


export const Dashboard: React.FC<{ loader?: BaseAxiosApiLoader }> = (props) => {

  const [stats, setStats] = React.useState<BasicStats | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const styles = useStyles();

  React.useEffect(() => {
    if (props.loader)
      getBasicStats(props.loader).then((d) => {
        setStats(d);
      }).catch((e: Error) => {
        console.error("Error: ", e);
        setError(e.toString());
      });
  }, [props.loader]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Copilot Feedback Bot</h1>

          <p>Welcome to the copilot feedback bot control panel app.</p>

          {error ? <p className='error'>{error}</p>
            :
            <>
              {stats ?
                <div>
                  <ChartContainer>

                    <div className='nav'>
                      <ul>
                        <li>
                          <Card className={styles.card}>
                            <CardHeader
                              header={<Text weight="semibold">Survey Stats</Text>}
                              description={
                                <Caption1 className={styles.caption}>Responded vs surveyed</Caption1>
                              }
                              action={
                                <Button
                                  appearance="transparent"
                                  icon={<MoreHorizontal20Regular />}
                                  aria-label="More options"
                                />
                              }
                            />

                            <p className={styles.text}>
                              Users are surveyed for copilot interactions. They don't necesarily reply.
                            </p>

                            <SurveyStatsChart stats={stats} />
                          </Card>
                        </li>
                        <li>
                          <Card className={styles.card}>
                            <CardHeader
                              header={<Text weight="semibold">Copilot Engagement</Text>}
                              description={
                                <Caption1 className={styles.caption}>Active Users vs Inactive</Caption1>
                              }
                              action={
                                <Button
                                  appearance="transparent"
                                  icon={<MoreHorizontal20Regular />}
                                  aria-label="More options"
                                />
                              }
                            />

                            <p className={styles.text}>
                              Some users are more active than others. This chart shows the distribution.
                            </p>

                            <UserStatsChart stats={stats} />
                          </Card>
                        </li>
                      </ul>
                    </div>
                  </ChartContainer>
                </div> : <Spinner />
              }
            </>
          }

        </div >
      </section >

    </div >
  );
};
