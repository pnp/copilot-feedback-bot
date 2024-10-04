import React from "react";
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

export const SurveyPageAndQuestionsEdit: React.FC<SurveyPageAndQuestionsEditProps> = (props) => {

  const onSave = React.useCallback(() => {
    props.onPageSave();
  }, [props.onPageSave, props.page]);

  const onEditCancel = React.useCallback(() => {
    props.onEditCancel();
  }, [props.onEditCancel, props.page]);

  const [selectedTabValue, setSelectedTabValue] = React.useState<TabValue>("SurveyPageEditPage");

  const onTabSelect = (_event: SelectTabEvent, data: SelectTabData) => {
    setSelectedTabValue(data.value);
  };
  const styles = useStyles();

  return (
    <div>
      <h2>{props.page.name} - Edit</h2>
      <TabList selectedValue={selectedTabValue} onTabSelect={onTabSelect}>
        <Tab id="SurveyPageEditPage" value="SurveyPageEditPage">
          Survey Page
        </Tab>
        <Tab id="SurveyPageEditQuestions" value="SurveyPageEditQuestions">
          Questions
        </Tab>
      </TabList>
      <div className={styles.panels}>
        {selectedTabValue === "SurveyPageEditPage" &&
          <EditSurveyPage onPageEdited={props.onPageEdited} page={props.page} />}
        {selectedTabValue === "SurveyPageEditQuestions" &&
          <EditSurveyQuestions {...props} page={props.page} />}
      </div>

      <div className='nav'>
        <ul>
          <li>
            <Button appearance="primary" onClick={onSave}>Save Page Changes</Button>
          </li>
          <li>
            <Button appearance="secondary" onClick={onEditCancel}>Cancel</Button>
          </li>
        </ul>
      </div>
    </div>
  );
};
