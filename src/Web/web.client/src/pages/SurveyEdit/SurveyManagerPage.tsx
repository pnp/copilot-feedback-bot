
import React from 'react';
import { BaseApiLoader } from '../../api/ApiLoader';
import { getSurveyPages } from '../../api/ApiCalls';
import { SurveyPageDB, SurveyPageEditViewModel, SurveyQuestionDB } from '../../apimodels/Models'; // Ensure SurveyPageDB is a class or constructor function
import { SurveyPageAndQuestionsEdit } from './SurveyPageAndQuestionsEdit';
import { Link, Spinner } from '@fluentui/react-components';

import update from 'immutability-helper';
import { SurveyPagesViewList } from './SurveyPagesViewList';
import isEqual from 'lodash.isequal';

export const SurveyManagerPage: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const [surveyPages, setSurveyPages] = React.useState<SurveyPageEditViewModel[] | null>(null);
  const [editingSurveyPage, setEditingSurveyPage] = React.useState<SurveyPageEditViewModel | null>(null);

  // Load survey pages from the server
  React.useEffect(() => {
    if (props.loader)
      getSurveyPages(props.loader).then((r) => {
        setSurveyPages(r);
      });
  }, [props.loader]);

  const updateSurveyPages = React.useCallback((pages: SurveyPageEditViewModel[]) => {
    setSurveyPages(pages);
    console.debug("Updated pages data: ", pages);
  }, []);

  const startEditPage = React.useCallback((page: SurveyPageEditViewModel | null) => {
    if (!page) 
      console.debug("Cancel editing page");
    else
      console.debug("Start editing page: ", page);
    setEditingSurveyPage(page);
  }, [editingSurveyPage]);

  const onNewPage = React.useCallback(() => {
    if (surveyPages) {
      console.debug("Creating new page");
      const newPage: SurveyPageEditViewModel = {
        name: 'New Page',
        adaptiveCardTemplateJson: '{}',
        adaptiveCardTemplateJsonWithQuestions: '{}',
        pageIndex: surveyPages.length,
        questions: [],
        isPublished: false
      };
      updateSurveyPages([...surveyPages, newPage]);
      startEditPage(newPage);
    }
  }, [surveyPages]);

  const onPageEdited = React.useCallback((page: SurveyPageEditViewModel) => {
    if (!surveyPages || !editingSurveyPage) return;

    
    if (isEqual(page, editingSurveyPage)) {
      console.debug("No page changes detected");
    }

    if (!page.id) {
      console.debug("New page save: ", page);
    }
    else {
      console.debug("Updated page: ", page);
}
    var pageIndex = surveyPages.findIndex((p) => p.id === page.id);
    const updatedPages = update(surveyPages, { [pageIndex]: { $set: page } });
    updateSurveyPages(updatedPages);

    // Find the updated page
    const updatedPageIndex = updatedPages.findIndex((p) => p.id === page.id);
    const updatedPage = updatedPages[updatedPageIndex];
    
    // Update page being edited
    setEditingSurveyPage(updatedPage);

  }, [surveyPages, editingSurveyPage]);

  const onPageDeleted = React.useCallback((page: SurveyPageDB) => {
    console.debug("Deleting page: ", page);

    if (!surveyPages) return;
    var pageIndex = surveyPages.findIndex((p) => p.id === page.id);
    var updatedPages = update(surveyPages, { $splice: [[pageIndex, 1]] });
    updateSurveyPages(updatedPages);
  }, [surveyPages]);

  const onQuestionEditedOrCreated = React.useCallback((q: SurveyQuestionDB) => {
    if (!surveyPages) return;

    var pageIndex = surveyPages.findIndex((p) => p.id === q.forSurveyPageId);
    let updatedPages: SurveyPageEditViewModel[] = [];
    if (!q.id) {
      console.debug("New question: ", q);
      if (pageIndex === -1) return;

      updatedPages = update(surveyPages, { [pageIndex]: { questions: { $push: [q] } } });
    }
    else {
      if (pageIndex === -1) return;
      console.debug("Updated question: ", q);
      const page = surveyPages[pageIndex];
      var questionIndex = page.questions.findIndex((qq) => qq.id === q.id);
      if (questionIndex === -1) return;

      updatedPages = update(surveyPages, { [pageIndex]: { questions: { [questionIndex]: { $set: q } } } });
    }
    updateSurveyPages(updatedPages);

    // Find the updated page
    const updatedPageIndex = updatedPages.findIndex((p) => p.id === q.forSurveyPageId);
    const updatedPage = updatedPages[updatedPageIndex];

    // Update page being edited
    setEditingSurveyPage(updatedPage);
  }, [surveyPages]);

  const onPageQuestionDeleted = React.useCallback((q: SurveyQuestionDB) => {
    console.debug("Deleted question: ", q);
    if (!surveyPages) return;

    var pageIndex = surveyPages.findIndex((p) => p.id === q.forSurveyPageId);
    if (pageIndex === -1) return;

    const pageOld = surveyPages[pageIndex];
    var questionIndex = pageOld.questions.findIndex((qq) => qq.id === q.id);
    if (questionIndex === -1) return;
    const updatedPages = update(surveyPages, { [pageIndex]: { questions: { $splice: [[questionIndex, 1]] } } });
    updateSurveyPages(updatedPages);

    // Find the updated page
    const updatedPageIndex = updatedPages.findIndex((p) => p.id === q.forSurveyPageId);
    const updatedPage = updatedPages[updatedPageIndex];

    // Update page being edited
    setEditingSurveyPage(updatedPage);
  }, [surveyPages]);

  const onPageSave = React.useCallback(() => {
    if (!editingSurveyPage) return;

    console.debug("Saving page: ", editingSurveyPage);
    startEditPage(null);
  }, []);

  return (
    <div className='surveyPage'>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Edit the questions the bot sends to users about copilot.</p>
          {editingSurveyPage ?
            <SurveyPageAndQuestionsEdit page={editingSurveyPage} onPageEdited={onPageEdited} onPageDeleted={onPageDeleted}
              onQuestionDeleted={onPageQuestionDeleted} onQuestionEdited={onQuestionEditedOrCreated}
              onEditCancel={() => startEditPage(null)} onPageSave={onPageSave} />
            :
            <>
              {surveyPages ?
                <>
                  <SurveyPagesViewList pages={surveyPages} onStartEdit={startEditPage} onDelete={onPageDeleted} />

                  <Link onClick={onNewPage}>Add new survey page</Link>
                </>
                :
                <Spinner />
              }
            </>
          }

        </div>
      </section>

    </div >
  );
};
