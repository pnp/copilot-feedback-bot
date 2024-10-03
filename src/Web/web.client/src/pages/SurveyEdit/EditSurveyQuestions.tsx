import React from "react";
import { SurveyPageDB, SurveyQuestionDB } from "../../apimodels/Models";
import { Link } from "@fluentui/react-components";
import { SurveyQuestionForm } from "./SurveyQuestionForm";
import { QuestionDatatype } from "../../apimodels/Enums";

export const EditSurveyQuestions: React.FC<{ page: SurveyPageDB, onQuestionEdited: Function, onQuestionDeleted: Function }> = (props) => {

  console.log("EditSurveyQuestions: ", props);

  return (
    <div className="pageEditTab">
      <div>
        {props.page.questions.length > 0 ?
          <>
            {props.page.questions.map((q) => {
              return <SurveyQuestionForm key={q.id} q={q} {...props} />
            })}
          </>
          :
          <div>No questions for this page</div>
        }
      </div>

      <Link onClick={() => {
        if (!props.page.id) return;
        const newQuestion: SurveyQuestionDB =
        {
          question: "New Question", questionId: "0",
          dataType: QuestionDatatype.String,
          forSurveyPageId: props.page.id
        };
        props.onQuestionEdited(newQuestion);
      }}>
        Add new question
      </Link>
    </div>
  );
};
