
import React from 'react';
import { SurveyPageEditViewModel } from '../../apimodels/Models'; // Ensure SurveyPageDB is a class or constructor function
import { QuestionsList } from './QuestionsList';

export const SurveyPagesList: React.FC<{ pages: SurveyPageEditViewModel[] }> = (props) => {


  return (
    <>
      <h2>Pages List</h2>
      {props.pages.map((page) => {
        return <div>
          {page.name}
          <QuestionsList qs={page.questions} />
        </div>;
      })}
    </>
  );
};
