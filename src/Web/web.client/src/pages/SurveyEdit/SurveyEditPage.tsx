
import React from 'react';
import { BaseApiLoader } from '../../api/ApiLoader';
import { getSurveyPages } from '../../api/ApiCalls';
import { SurveyPageDB } from '../../apimodels/Models';
import { SurveyPage } from './SurveyPage';

export const SurveyEditPage: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const [surveyPages, setSurveyPages] = React.useState<SurveyPageDB[] | null>(null);
  const [surveyPageEdit, setSurveyPageEdit] = React.useState<SurveyPageDB | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getSurveyPages(props.loader).then((r) => {
        setSurveyPages(r);
      });
  }, [props.loader]);

  const deletePage = React.useCallback((page: SurveyPageDB) => {
  }, [surveyPages]);

  const updatePage = React.useCallback((page: SurveyPageDB) => {
  }, [surveyPages]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Edit the questions the bot sends to users about copilot.</p>
          {surveyPages ?
            <p>
              {surveyPages.map((page) => {
                return <SurveyPage key={page.id} page={page} onDelete={deletePage} onEdited={updatePage} />;
              })}
            </p>
            :
            <p>Loading...</p>
          }

        </div>
      </section>

    </div >
  );
};
