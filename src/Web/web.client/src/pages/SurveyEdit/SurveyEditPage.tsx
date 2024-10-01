
import React from 'react';
import { BaseApiLoader } from '../../api/ApiLoader';
import { getSurveyPages } from '../../api/ApiCalls';
import { SurveyPageDB } from '../../apimodels/Models';

export const SurveyEditPage: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const [surveyPages, setSurveyPages] = React.useState<SurveyPageDB[] | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getSurveyPages(props.loader).then((r) => {
        setSurveyPages(r);
      });
  }, [props.loader]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Edit the questions the bot sends to users about copilot.</p>
          {surveyPages ?
            <p>There are {surveyPages.length} survey pages.</p>
            :
            <p>Loading...</p>
          }

        </div>
      </section>

    </div >
  );
};
