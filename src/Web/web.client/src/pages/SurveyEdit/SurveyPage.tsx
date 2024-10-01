import React from "react";
import { SurveyPageDB } from "../../apimodels/Models";
import { AdaptiveCard } from "./AdaptiveCard";
import { Button, Field, Textarea } from "@fluentui/react-components";


export const SurveyPage: React.FC<{ page: SurveyPageDB, onEdited: Function, onDelete: Function }> = (props) => {

  const [editMode, setEditMode] = React.useState<boolean>(false);

  const onEditToggle = React.useCallback(() => {
    setEditMode(!editMode);
  }, [editMode]);

  const onDelete = React.useCallback(() => {
  }, [props.onDelete]);

  return (
    <div>
      <h2>{props.page.name}</h2>

      {editMode ?
        <>
          <Field label="Adaptive card JSon">
            <Textarea>{props.page.adaptiveCardTemplateJson}</Textarea>
          </Field>
          <div className='nav'>
            <ul>
              <li>
                <Button appearance="primary" onClick={onEditToggle}>Cancel</Button>
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
