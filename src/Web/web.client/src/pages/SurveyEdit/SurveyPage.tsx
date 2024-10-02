import React from "react";
import { SurveyPageEditViewModel } from "../../apimodels/Models";
import { AdaptiveCard } from "./AdaptiveCard";
import { Button, makeStyles, SelectTabData, SelectTabEvent, Tab, TabList, TabValue, tokens } from "@fluentui/react-components";
import { SurveyPageEditQuestions } from "./SurveyPageEditQuestions";
import { SurveyPageEditPage } from "./SurveyPageEditPage";


const useStyles = makeStyles({
  root: {
    alignItems: "flex-start",
    display: "flex",
    flexDirection: "column",
    justifyContent: "flex-start",
    padding: "50px 20px",
    rowGap: "20px",
  },
  panels: {
    padding: "0 10px",
    "& th": {
      textAlign: "left",
      padding: "0 30px 0 0",
    },
  },
  propsTable: {
    "& td:first-child": {
      fontWeight: tokens.fontWeightSemibold,
    },
    "& td": {
      padding: "0 30px 0 0",
    },
  },
});

export const SurveyPage: React.FC<{ page: SurveyPageEditViewModel, onEdited: Function, onDelete: Function }> = (props) => {

  const [editMode, setEditMode] = React.useState<boolean>(false);

  const onEditToggle = React.useCallback(() => {
    setEditMode(!editMode);
  }, [editMode]);

  const onDelete = React.useCallback(() => {
  }, [props.onDelete]);



  const [selectedValue, setSelectedValue] = React.useState<TabValue>("SurveyPageEditPage");

  const onTabSelect = (_event: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
  };
  const styles = useStyles();

  return (
    <div>
      <h2>{props.page.name}</h2>

      {editMode ?
        <>

          <TabList selectedValue={selectedValue} onTabSelect={onTabSelect}>
            <Tab id="SurveyPageEditPage" value="SurveyPageEditPage">
              Survey Page
            </Tab>
            <Tab id="SurveyPageEditQuestions" value="SurveyPageEditQuestions">
              Questions
            </Tab>
          </TabList>
          <div className={styles.panels}>
            {selectedValue === "SurveyPageEditPage" && <SurveyPageEditPage onDelete={props.onDelete} onEdited={props.onEdited} page={props.page} />}
            {selectedValue === "SurveyPageEditQuestions" && <SurveyPageEditQuestions
              onDelete={props.onDelete} onEdited={props.onEdited} page={props.page} />}
          </div>


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
        </>
      }

    </div>
  );
};
