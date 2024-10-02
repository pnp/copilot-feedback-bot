import React from "react";
import { SurveyPageDB, SurveyQuestionDB } from "../../apimodels/Models";
import { Link } from "@fluentui/react-components";
import { SurveyQuestion } from "./SurveyQuestion";
import { QuestionDatatype } from "../../apimodels/Enums";

export const SurveyPageEditQuestions: React.FC<{ page: SurveyPageDB, onEdited: Function, onDelete: Function }> = (props) => {

  const [pageQuestions, setPageQuestions] = React.useState<SurveyQuestionDB[]>(props.page.questions);

  const onDeleteQuestion = React.useCallback((q: SurveyQuestionDB) => {
    console.log("Deleting question: ", q);
    const newQuestions = pageQuestions.filter((question) => question.id !== q.id);
    console.log("New questions: ", newQuestions);
    setPageQuestions(newQuestions);
  }, [props.onDelete]);


  return (
    <div className="pageEditTab">
      <div>
        {pageQuestions.length > 0 ?
          <>
            {pageQuestions.map((q) => {
              return <SurveyQuestion key={q.id} q={q} deleteQuestion={() => onDeleteQuestion(q)} />
            })}
          </>
          :
          <div>No questions for this page</div>
        }
      </div>

      <Link onClick={() => {
        const newQuestion: SurveyQuestionDB = { id: "0", question: "New Question", questionId: "0", dataType: QuestionDatatype.String };
        setPageQuestions([...pageQuestions, newQuestion]);
      }}>
        Add new question
      </Link>
    </div>
  );
};
