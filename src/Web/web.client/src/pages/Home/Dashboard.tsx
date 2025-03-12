
import React from 'react';
import 'chartjs-adapter-date-fns'
import { getBasicStats } from '../../api/ApiCalls';
import { BasicStats } from '../../apimodels/Models';
import { Button, Caption1, Card, CardHeader, makeStyles, Spinner, Text, tokens } from '@fluentui/react-components';
import { SurveyStatsChart } from './SurveyStatsChart';
import { ChartContainer } from '../../components/app/ChartContainer';
import { UserStatsChart } from './UserStatsChart';
import { MoreHorizontal20Regular } from "@fluentui/react-icons";
import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';


const useStyles = makeStyles({
  main: {
    gap: "36px",
    display: "flex",
    flexDirection: "column",
    flexWrap: "wrap",
  },

  card: {
    width: "360px",
    maxWidth: "100%",
    height: "fit-content",
  },

  section: {
    width: "fit-content",
  },

  title: { margin: "0 0 12px" },

  horizontalCardImage: {
    width: "64px",
    height: "64px",
  },

  headerImage: {
    borderRadius: "4px",
    maxWidth: "44px",
    maxHeight: "44px",
  },

  caption: {
    color: tokens.colorNeutralForeground3,
  },

  text: { margin: "0" },
});

export const Dashboard: React.FC<{ loader?: BaseAxiosApiLoader }> = (props) => {

  const [basicStats, setBasicStats] = React.useState<BasicStats | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const styles = useStyles();

  React.useEffect(() => {
    if (props.loader)
      getBasicStats(props.loader).then((d) => {
        setBasicStats(d);
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
              {basicStats ?
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

                            <SurveyStatsChart stats={basicStats} />
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

                            <UserStatsChart stats={basicStats} />
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
