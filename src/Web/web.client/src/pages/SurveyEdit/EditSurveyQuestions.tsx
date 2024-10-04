import React from "react";
import { SurveyQuestionDTO } from "../../apimodels/Models";
import { Link } from "@fluentui/react-components";
import { SurveyQuestionForm } from "./SurveyQuestionForm";
import { QuestionDatatype } from "../../apimodels/Enums";
import { GetSurveyQuestionDTO } from "./CommonFunctions";

export const EditSurveyQuestions: React.FC<EditSurveyQuestionsProps> = (props) => {

  return (
    <div className="pageEditTab">
      <div>
        {props.page.questions.length > 0 ?
          <>
            {props.page.questions.map((q: SurveyQuestionDTO, i: number) => {
              return <SurveyQuestionForm key={i} q={q} {...props} />
            })}
          </>
          :
          <div>No questions for this page</div>
        }
      </div>

      {
        !props.page.id ?
          <div>Save the page to add questions</div> :
          <Link onClick={() => {
            if (!props.page.id) return;

            const newQuestion: SurveyQuestionDTO =
            {
              question: "New Question", questionId: "0",
              dataType: QuestionDatatype.String,
              forSurveyPageId: props.page.id,
            };
            props.onQuestionEdited(GetSurveyQuestionDTO(newQuestion));
          }}>
            Add new question
          </Link>
      }
    </div>
  );
};
