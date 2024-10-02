import React from "react";
import { SurveyPageEditViewModel } from "../../apimodels/Models";
import { Button, makeStyles, SelectTabData, SelectTabEvent, Tab, TabList, TabValue, tokens } from "@fluentui/react-components";
import { EditSurveyQuestions } from "./EditSurveyQuestions";
import { EditSurveyPage } from "./EditSurveyPage";


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

export const SurveyPageEdit: React.FC<{ page: SurveyPageEditViewModel, onPageEdited: Function, onQuestionEdited: Function, onPageDeleted: Function, onQuestionDeleted: Function }> = (props) => {

  const onSave = React.useCallback(() => {
    console.log("Saving page: ", props.page);
    props.onPageEdited(props.page);
  }, [props.onPageEdited, props.page]);

  const [selectedValue, setSelectedValue] = React.useState<TabValue>("SurveyPageEditPage");

  const onTabSelect = (_event: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
  };
  const styles = useStyles();

  return (
    <div>
      <h2>{props.page.name}</h2>

          <TabList selectedValue={selectedValue} onTabSelect={onTabSelect}>
            <Tab id="SurveyPageEditPage" value="SurveyPageEditPage">
              Survey Page
            </Tab>
            <Tab id="SurveyPageEditQuestions" value="SurveyPageEditQuestions">
              Questions
            </Tab>
          </TabList>
          <div className={styles.panels}>
            {selectedValue === "SurveyPageEditPage" && 
              <EditSurveyPage onPageEdited={props.onPageEdited} page={props.page} />}
            {selectedValue === "SurveyPageEditQuestions" && 
              <EditSurveyQuestions onQuestionEdited={props.onQuestionEdited} onQuestionDeleted={props.onQuestionDeleted} page={props.page} />}
          </div>


          <div className='nav'>
            <ul>
              <li>
                <Button appearance="primary" onClick={onSave}>Save</Button>
              </li>
            </ul>
          </div>
    </div>
  );
};
