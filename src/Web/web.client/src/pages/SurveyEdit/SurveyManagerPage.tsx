
import React from 'react';
import { BaseApiLoader } from '../../api/ApiLoader';
import { getSurveyPages } from '../../api/ApiCalls';
import { SurveyPageDB, SurveyPageEditViewModel, SurveyQuestionDB } from '../../apimodels/Models'; // Ensure SurveyPageDB is a class or constructor function
import { SurveyPageEdit } from './SurveyPageEdit';
import { Button, Spinner } from '@fluentui/react-components';
import { SurveyPageView } from './SurveyPageView';

import update from 'immutability-helper';

export const SurveyManagerPage: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const [surveyPages, setSurveyPages] = React.useState<SurveyPageEditViewModel[] | null>(null);
  const [editingSurveyPage, setEditingSurveyPage] = React.useState<SurveyPageEditViewModel | null>(null);

  React.useEffect(() => {
    if (props.loader)
      getSurveyPages(props.loader).then((r) => {
        setSurveyPages(r);
      });
  }, [props.loader]);

  const deletePage = React.useCallback((page: SurveyPageDB) => {
    console.log("Deleting page: ", page);

    if (!surveyPages) return;
    var pageIndex = surveyPages.findIndex((p) => p.id === page.id);
    var updatedPages = update(surveyPages, { $splice: [[pageIndex, 1]] });
    setSurveyPages(updatedPages);
    console.log("Updated pages: ", updatedPages);
  }, [surveyPages]);

  const startEditPage = React.useCallback((page: SurveyPageEditViewModel | null) => {
    console.log("Start editing page: ", page);
    setEditingSurveyPage(page);
  }, [editingSurveyPage]);

  const onPageEdited = React.useCallback((page: SurveyPageEditViewModel) => {
    console.log("Updated page: ", page);
    if (!surveyPages) return;

    var pageIndex = surveyPages.findIndex((p) => p.id === page.id);
    const updatedPages = update(surveyPages, { [pageIndex]: { $set: page } });
    setSurveyPages(updatedPages);
    console.log("Updated pages: ", updatedPages);

  }, [surveyPages]);


  const onPageDeleted = React.useCallback((page: SurveyPageDB) => {
    console.log("Deleted page: ", page);
  }, [surveyPages]);


  const onQuestionEdited = React.useCallback((q: SurveyQuestionDB) => {
    console.log("Updated question: ", q);
    if (!surveyPages) return;

    var pageIndex = surveyPages.findIndex((p) => p.id === q.forSurveyPageId);
    if (pageIndex === -1) return;

    const page = surveyPages[pageIndex];
    var questionIndex = page.questions.findIndex((qq) => qq.id === q.id);
    if (questionIndex === -1) return;
    const updatedPages = update(surveyPages, { [pageIndex]: { questions: { [questionIndex]: { $set: q } } } });
    setSurveyPages(updatedPages);
    console.log("Updated pages: ", updatedPages);

  }, [surveyPages]);


  const onQuestionDeleted = React.useCallback((q: SurveyQuestionDB) => {
    console.log("Deleted question: ", q);
    if (!surveyPages) return;

    var pageIndex = surveyPages.findIndex((p) => p.id === q.forSurveyPageId);
    if (pageIndex === -1) return;

    const page = surveyPages[pageIndex];
    var questionIndex = page.questions.findIndex((qq) => qq.id === q.id);
    if (questionIndex === -1) return;
    const updatedPages = update(surveyPages, { [pageIndex]: { questions: { $splice: [[questionIndex, 1]] } } });
    setSurveyPages(updatedPages);
    console.log("Updated pages: ", updatedPages);
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
      setEditingSurveyPage(newPage);
    }
  }, [surveyPages]);

  return (
    <div className='surveyPage'>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Edit the questions the bot sends to users about copilot.</p>
          {editingSurveyPage ?
            <SurveyPageEdit page={editingSurveyPage} onPageEdited={onPageEdited} onPageDeleted={onPageDeleted}
              onQuestionDeleted={onQuestionDeleted} onQuestionEdited={onQuestionEdited} 
              onEditCancel={()=> startEditPage(null)}/>
            :
            <>
              {surveyPages ?
                <>
                  {surveyPages.map((page) => {
                    return <SurveyPageView key={page.id} onStartEdit={startEditPage} page={page} onDelete={deletePage} />;
                  })}
                </>
                :
                <Spinner />
              }
            </>
          }

          <p>Add pages to the questionaire list:</p>
          <Button appearance="primary" onClick={onNewPage}>New Page</Button>
        </div>
      </section>

    </div >
  );
};
