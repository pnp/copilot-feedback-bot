import React from 'react';
import { getSurveysReport } from '../../api/ApiCalls';
import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { SurveysReport } from '../../apimodels/Models';
import { Caption1, Card, CardHeader, Spinner, Text } from '@fluentui/react-components';
import { AnswersPieChart } from './AnswersPieChart';
import { useStyles } from '../../utils/styles';


export function AdoptionHome(props: { loader?: BaseAxiosApiLoader }) {
  const [error, setError] = React.useState<string | null>(null);
  const [stats, setStats] = React.useState<SurveysReport | null>(null);
  const styles = useStyles();

  React.useEffect(() => {
    if (props.loader)
      getSurveysReport(props.loader).then((d) => {
        setStats(d);
      }).catch((e: Error) => {
        console.error("Error: ", e);
        setError(e.toString());
      });
  }, [props.loader]);

  return (
    <div>
      <h1>Adoption</h1>
      <p>Graphs to show copilot adoption.</p>
      {error ? <p className='error'>{error}</p>
        :
        <>
          {stats ?
            <div>
              <h2>Survey Answers</h2>

              {stats ?
                <div>

                  <div className='nav'>
                    <ul>
                      <li>
                        <Card className={styles.card}>
                          <CardHeader
                            header={<Text weight="semibold">Positive vs Negative Responses</Text>}
                          />

                          <AnswersPieChart stats={stats} />
                        </Card>
                      </li>
                      <li>
                        <Card className={styles.card}>
                          <CardHeader
                            header={<Text weight="semibold">Most positive question</Text>}
                            description={
                              <Caption1 className={styles.caption}>The question to which users gave the most optimal response</Caption1>
                            }
                          />

                          <p className={styles.text}>
                            Question: '{stats.stats.highestPositiveAnswerQuestion.entity}'
                          </p>
                          <p className={styles.text}>
                            {stats.stats.highestPositiveAnswerQuestion.score} response(s)
                          </p>
                        </Card>
                      </li>
                      <li>
                        <Card className={styles.card}>
                          <CardHeader
                            header={<Text weight="semibold">Least positive question</Text>}
                            description={
                              <Caption1 className={styles.caption}>The question to which users gave the least optimal response</Caption1>
                            }
                          />

                          <p className={styles.text}>
                            Question: '{stats.stats.highestNegativeAnswerQuestion.entity}'
                          </p>
                          <p className={styles.text}>
                            {stats.stats.highestNegativeAnswerQuestion.score} response(s)
                          </p>
                        </Card>
                      </li>
                    </ul>
                  </div>
                </div>
                :
                <Spinner />
              }
            </div>
            :
            <Spinner />
          }
        </>
      }
    </div>
  );
}
