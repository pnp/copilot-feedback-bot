import React from "react";
import { SurveyPageDB, SurveyQuestionDB } from "../../apimodels/Models";
import { Link } from "@fluentui/react-components";
import { SurveyQuestion, SurveyQuestionProps, SurveyQuestionRef } from "./SurveyQuestion";
import { QuestionDatatype } from "../../apimodels/Enums";

export const EditSurveyQuestions: React.FC<{ page: SurveyPageDB, onQuestionEdited: Function, onQuestionDeleted: Function }> = (props) => {

  const [pageQuestions, setPageQuestions] = React.useState<SurveyQuestionDB[]>(props.page.questions);

  const [pageQuestionRefs, setPageQuestionRefs] = React.useState<React.RefObject<SurveyQuestionRef>[]>([]);

  const onDeleteQuestion = React.useCallback((q: SurveyQuestionDB) => {
    console.log("Deleting question: ", q);
    const newQuestions = pageQuestions.filter((question) => question.id !== q.id);
    console.log("New questions: ", newQuestions);
    setPageQuestions(newQuestions);
  }, [props.onQuestionDeleted, pageQuestions]);

  // Create refs for each question
  React.useEffect(() => {
    const refs = pageQuestions.map(() => React.createRef<SurveyQuestionRef>());
    setPageQuestionRefs(refs);
  }, []);

  return (
    <div className="pageEditTab">
      <div>
        {pageQuestions.length > 0 ?
          <>
            {pageQuestions.map((q) => {
              return <SurveyQuestion key={q.id} q={q} />
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


      <Link onClick={() => {
        console.log("Saving all questions");
        pageQuestionRefs.forEach((ref) => {
          if (ref.current) {
            const q = ref.current.getQuestion();
            console.log("New question content: ", q);
            props.onQuestionEdited(q);
          }
        });
      }}>
        Save All
      </Link>
    </div>
  );
};
