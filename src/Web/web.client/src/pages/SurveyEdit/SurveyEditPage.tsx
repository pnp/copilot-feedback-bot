
import React from 'react';
import { BaseApiLoader } from '../../api/ApiLoader';
import { getSurveyPages } from '../../api/ApiCalls';
import { SurveyPageDB, SurveyPageEditViewModel } from '../../apimodels/Models'; // Ensure SurveyPageDB is a class or constructor function
import { SurveyPage } from './SurveyPage';
import { Button } from '@fluentui/react-components';

export const SurveyEditPage: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const [surveyPages, setSurveyPages] = React.useState<SurveyPageEditViewModel[] | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getSurveyPages(props.loader).then((r) => {
        setSurveyPages(r);
      });
  }, [props.loader]);

  const deletePage = React.useCallback((page: SurveyPageDB) => {
    console.log("Deleting page: ", page);
  }, [surveyPages]);

  const updatedPage = React.useCallback((page: SurveyPageDB) => {
    console.log("Updated page: ", page);
  }, [surveyPages]);

  const onNewPage = React.useCallback(() => {
    if (surveyPages) {
      const newPage: SurveyPageEditViewModel = {
        id: '', // Add appropriate default values for the properties
        name: 'New Page',
        adaptiveCardTemplateJson: '{}',
        adaptiveCardTemplateJsonWithQuestions: '{}',
        pageIndex: surveyPages.length,
        questions: [],
        isPublished: false
      };
      setSurveyPages([...surveyPages, newPage]);
      
    }
  }, [surveyPages]);

  return (
    <div className='surveyPage'>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Edit the questions the bot sends to users about copilot.</p>
          {surveyPages ?
            <>
              {surveyPages.map((page) => {
                return <SurveyPage key={page.id} page={page} onDelete={deletePage} onEdited={updatedPage} />;
              })}
            </>
            :
            <p>Loading...</p>
          }

          <p>Add pages to the questionaire list:</p>
          <Button appearance="primary" onClick={onNewPage}>New Page</Button>
        </div>
      </section>

    </div >
  );
};
