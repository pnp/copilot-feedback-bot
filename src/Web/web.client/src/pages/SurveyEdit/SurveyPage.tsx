import React from "react";
import { SurveyPageDB, SurveyQuestionDB } from "../../apimodels/Models";
import { AdaptiveCard } from "./AdaptiveCard";
import { Button, Checkbox, Field, Input, Link, Textarea } from "@fluentui/react-components";
import { SurveyQuestion } from "./SurveyQuestion";
import { QuestionDatatype } from "../../apimodels/Enums";


export const SurveyPage: React.FC<{ page: SurveyPageDB, onEdited: Function, onDelete: Function }> = (props) => {

  const [editMode, setEditMode] = React.useState<boolean>(false);

  const [pageIsPublished, setPageIsPublished] = React.useState<boolean>(props.page.isPublished);
  const [pageName, setPageName] = React.useState<string>(props.page.name);
  const [pageIndex, setPageIndex] = React.useState<number>(props.page.pageIndex);
  const [pageJson, setPageJson] = React.useState<string>(props.page.adaptiveCardTemplateJson);
  
  const [pageQuestions, setPageQuestions] = React.useState<SurveyQuestionDB[]>(props.page.questions);

  const onEditToggle = React.useCallback(() => {
    setEditMode(!editMode);
  }, [editMode]);

  const onDelete = React.useCallback(() => {
  }, [props.onDelete]);

  
  const onDeleteQuestion = React.useCallback((q: SurveyQuestionDB) => {
    console.log("Deleting question: ", q);
    const newQuestions = pageQuestions.filter((question) => question.id !== q.id);
    console.log("New questions: ", newQuestions);
    setPageQuestions(newQuestions);
  }, [props.onDelete]);

  return (
    <div>
      <h2>{props.page.name}</h2>

      {editMode ?
        <>
          <Field label="Page Name">
            <Input onChange={(e) => setPageName(e.target.value)} value={pageName} />
          </Field>

          <Field label="Page Index">
            <Input value={pageIndex.toString()} type="number" onChange={(e) => setPageIndex(Number(e.target.value))} />
          </Field>

          <Checkbox
            checked={pageIsPublished}
            onChange={(_ev, data) => setPageIsPublished(data.checked === true)}
            label="Published"
          />

          <Field label="Adaptive card JSon">
            <Textarea onChange={(e) => setPageJson(e.target.value)}>{pageJson}</Textarea>
          </Field>


          <Field label="Questions in page">
            {pageQuestions.length > 0 ?
              <>
                {pageQuestions.map((q) => {
                  return <SurveyQuestion key={q.id} q={q} deleteQuestion={() => onDeleteQuestion(q)} />
                })}
              </>
              :
              <div>No questions for this page</div>
            }
            <Link onClick={() => {
              const newQuestion: SurveyQuestionDB = { id: "0", question: "New Question", questionId: "0", dataType: QuestionDatatype.String };
              setPageQuestions([...pageQuestions, newQuestion]);
            }}>
              Add Question
            </Link>

          </Field>

          <div className='nav'>
            <ul>
              <li>
                <Button appearance="secondary" onClick={onEditToggle}>Cancel</Button>
              </li>
              <li>
                <Button appearance="primary" onClick={onDelete}>Save</Button>
              </li>
            </ul>
          </div>
        </>
        :
        <>
          <div>
            <AdaptiveCard json={props.page.adaptiveCardTemplateJson} />
          </div>
          <div className='nav'>
            <ul>
              <li>
                <Button appearance="primary" onClick={onEditToggle}>Edit</Button>
              </li>
              <li>
                <Button appearance="primary" onClick={onDelete}>Delete</Button>
              </li>
            </ul>
          </div>
        </>
      }

    </div>
  );
};
