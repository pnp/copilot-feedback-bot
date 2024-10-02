import React from "react";
import { SurveyPageEditViewModel } from "../../apimodels/Models";
import { AdaptiveCard } from "./AdaptiveCard";
import { Button } from "@fluentui/react-components";


export const SurveyPageView: React.FC<{ page: SurveyPageEditViewModel, onStartEdit: Function, onDelete: Function }> = (props) => {

  const [editMode, setEditMode] = React.useState<boolean>(false);

  const onEditToggle = React.useCallback(() => {
    props.onStartEdit(props.page);
    setEditMode(!editMode);
  }, [editMode]);

  const onDelete = React.useCallback(() => {
  }, [props.onDelete]);


  return (
    <div>
      <h2>{props.page.name}</h2>

      <div>
        <AdaptiveCard json={props.page.adaptiveCardTemplateJsonWithQuestions} />
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

    </div>
  );
};
