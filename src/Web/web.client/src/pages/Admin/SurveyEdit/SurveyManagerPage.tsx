
import React from 'react';
import { deleteSurveyPage, getSurveyPages, saveSurveyPage } from '../../../api/ApiCalls';
import { SurveyPageDTO, SurveyQuestionDTO } from '../../../apimodels/Models'; // Ensure SurveyPageDTO is a class or constructor function
import { SurveyPageAndQuestionsEdit } from './SurveyPageAndQuestionsEdit';
import { Link, Spinner } from '@fluentui/react-components';

import update from 'immutability-helper';
import isEqual from 'lodash.isequal';
import { SurveyPageView } from './SurveyPageView';
import { BaseAxiosApiLoader } from '../../../api/AxiosApiLoader';

export const SurveyManagerPage: React.FC<{ loader?: BaseAxiosApiLoader }> = (props) => {

  const [haveNewPage, setHaveNewPage] = React.useState<boolean>(false);
  const [surveyPages, setSurveyPages] = React.useState<SurveyPageDTO[] | null>(null);
  const [editingSurveyPage, setEditingSurveyPage] = React.useState<SurveyPageDTO | null>(null);

  // Load survey pages from the server
  React.useEffect(() => {
    if (props.loader)
      getSurveyPages(props.loader).then((r) => {
        setSurveyPages(r);
      });
  }, [props.loader]);

  const updateSurveyPages = React.useCallback((pages: SurveyPageDTO[]) => {
    setSurveyPages(pages);
    console.debug("Updated pages data: ", pages);
  }, []);

  const startEditPage = React.useCallback((page: SurveyPageDTO | null) => {
    if (!page)
      console.debug("Cancel editing page");
    else
      console.debug("Start editing page: ", page);
    setEditingSurveyPage(page);
  }, [editingSurveyPage]);

  const onNewPage = React.useCallback(() => {
    console.debug("Creating new page");
    if (surveyPages) {
      const newPage: SurveyPageDTO = {
        name: 'New Page',
        adaptiveCardTemplateJson: '{}',
        pageIndex: surveyPages.length,
        questions: [],
        isPublished: false
      };
      updateSurveyPages([...surveyPages, newPage]);
      startEditPage(newPage);
      setHaveNewPage(true);
    }
  }, [surveyPages]);

  const onPageEdited = React.useCallback((page: SurveyPageDTO) => {
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

  const onPageDeleted = React.useCallback((page: SurveyPageDTO) => {
    console.debug("Deleting page: ", page);

    if (!surveyPages || !props.loader) return;

    // Set loading
    setSurveyPages(null);

    // Call API
    deleteSurveyPage(props.loader, page.id ?? '').then(() => {
      console.debug("Page deleted");

      // Remove the page from the view index
      var pageIndex = surveyPages.findIndex((p) => p.id === page.id);
      var updatedPages = update(surveyPages, { $splice: [[pageIndex, 1]] });
      updateSurveyPages(updatedPages);
    });
  }, [surveyPages]);

  const onQuestionEditedOrCreated = React.useCallback((q: SurveyQuestionDTO) => {
    if (!surveyPages) return;

    var pageIndex = surveyPages.findIndex((p) => p.id === q.forSurveyPageId);
    let updatedPages: SurveyPageDTO[] = [];
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

  const onPageQuestionDeleted = React.useCallback((q: SurveyQuestionDTO) => {
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

  // Save to server
  const onPageSave = React.useCallback(() => {
    console.debug("Saving page: ", editingSurveyPage);
    if (!editingSurveyPage || !props.loader) return;

    setSurveyPages(null);
    saveSurveyPage(props.loader, editingSurveyPage).then((r) => {
      setSurveyPages(r);
    });

    startEditPage(null);
    setHaveNewPage(false);
  }, [editingSurveyPage]);

  return (
    <div className='surveyPage'>
      <section className="page--header">
        <div className="page-title">
          <h1>Survey Editor</h1>

          <p>Edit the questions the bot sends to users about copilot.</p>
          {editingSurveyPage ?
            <SurveyPageAndQuestionsEdit page={editingSurveyPage} onPageFieldUpdated={onPageEdited}
              onQuestionDeleted={onPageQuestionDeleted} onQuestionEdited={onQuestionEditedOrCreated}
              onEditCancel={() => startEditPage(null)} onPageSave={onPageSave} />
            :
            <>
              {surveyPages ?
                <>
                  {surveyPages.map((page) => {
                    return <SurveyPageView key={page.id ?? 0} page={page}
                      onDelete={onPageDeleted} onStartEdit={startEditPage} />;
                  })}

                  {!haveNewPage &&
                    <Link onClick={onNewPage}>Add new survey page</Link>
                  }
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
