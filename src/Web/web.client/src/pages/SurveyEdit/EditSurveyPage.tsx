import React from "react";
import { SurveyPageDB } from "../../apimodels/Models";
import { Checkbox, Field, Input, Textarea } from "@fluentui/react-components";


export const EditSurveyPage: React.FC<{ page: SurveyPageDB, onPageEdited: Function }> = (props) => {

  const [pageIsPublished, setPageIsPublished] = React.useState<boolean>(props.page.isPublished);
  const [pageName, setPageName] = React.useState<string>(props.page.name);
  const [pageIndex, setPageIndex] = React.useState<number>(props.page.pageIndex);
  const [pageJson, setPageJson] = React.useState<string>(props.page.adaptiveCardTemplateJson);

  // Send updated question to parent
  React.useEffect(() => {
    const p: SurveyPageDB = {
      id: props.page.id,
      name: pageName,
      pageIndex: pageIndex,
      adaptiveCardTemplateJson: pageJson,
      isPublished: pageIsPublished,
      questions: props.page.questions
    };
    props.onPageEdited(p);
  }, [pageIsPublished, pageName, pageIndex, pageJson]);

  React.useEffect(() => {
    setPageIsPublished(props.page.isPublished);
    setPageName(props.page.name);
    setPageIndex(props.page.pageIndex);
    setPageJson(props.page.adaptiveCardTemplateJson);
    console.log("EditSurveyPage: Updated page data: ", props.page);
  }, [props.page]);

  return (
    <div className="pageEditTab">
      <Field label="Page Name">
        <Input onChange={(e) => setPageName(e.target.value)} value={pageName} />
      </Field>

      <Field label="Page Index">
        <Input value={pageIndex.toString()} type="number" onChange={(e) => setPageIndex(Number(e.target.value))} />
      </Field>

      <Checkbox
        checked={pageIsPublished}
        onChange={(_ev, data) => setPageIsPublished(data.checked === true)}
        label="Survey page published"
      />

      <Field label="Adaptive card JSon template (minus questions)">
        <Textarea style={{ height: 150 }} onChange={(e) => setPageJson(e.target.value)} value={pageJson}></Textarea>
      </Field>

    </div>
  );
};
