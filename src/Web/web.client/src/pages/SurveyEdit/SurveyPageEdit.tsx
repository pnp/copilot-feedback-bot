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

interface SurveyPageEditProps {
  page: SurveyPageEditViewModel,
  onPageEdited: Function, 
  onQuestionEdited: Function,
  onPageDeleted: Function, 
  onQuestionDeleted: Function,
  onEditCancel: Function,
}

export const SurveyPageEdit: React.FC<SurveyPageEditProps> = (props) => {

  const onSave = React.useCallback(() => {
    props.onPageEdited(props.page);
  }, [props.onPageEdited, props.page]);

  const onEditCancel = React.useCallback(() => {
    props.onEditCancel();
  }, [props.onPageEdited, props.page]);

  const [selectedTabValue, setSelectedTabValue] = React.useState<TabValue>("SurveyPageEditPage");

  
  const [page, setPage] = React.useState<SurveyPageEditViewModel>(props.page);

  React.useEffect(() => {
    setPage(props.page);
    console.log("Page updated: ", props.page);
  }, [props.page]);

  const onTabSelect = (_event: SelectTabEvent, data: SelectTabData) => {
    setSelectedTabValue(data.value);
  };
  const styles = useStyles();

  return (
    <div>
      <h2>{props.page.name} - Edit</h2>

      <div>{JSON.stringify(page)}</div>

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
            <Button appearance="primary" onClick={onSave}>Save</Button>
          </li>
          <li>
            <Button appearance="secondary" onClick={onEditCancel}>Cancel</Button>
          </li>
        </ul>
      </div>
    </div>
  );
};