import React from 'react';
import { getSurveyAnswers } from '../../api/ApiCalls';
import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { SurveyAnswersCollection } from '../../apimodels/Models';
import { Spinner } from '@fluentui/react-components';


export function AdoptionHome(props: { loader?: BaseAxiosApiLoader }) {
  const [error, setError] = React.useState<string | null>(null);
  const [stats, setStats] = React.useState<SurveyAnswersCollection | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getSurveyAnswers(props.loader).then((d) => {
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
              <pre>{JSON.stringify(stats, null, 2)}</pre>
            </div>
            :
            <Spinner />
          }
        </>
      }
    </div>
  );
}
