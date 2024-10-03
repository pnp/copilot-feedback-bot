
import React from 'react';
import { SurveyQuestionDB } from '../../apimodels/Models'; // Ensure SurveyPageDB is a class or constructor function

export const QuestionsList: React.FC<{ qs: SurveyQuestionDB[] }> = (props) => {


  return (
    <>
      <h2>Questions List</h2>
      {props.qs.map((q) => {
        return <div>{q.question}</div>;
      })}
    </>
  );
};
