
import React from 'react';
import { SurveyPageEditViewModel } from '../../apimodels/Models'; 
import { SurveyPageView } from './SurveyPageView';

export const SurveyPagesList: React.FC<{ pages: SurveyPageEditViewModel[], onStartEdit: Function, onDelete: Function }> = (props) => {

  const [pages, setPages] = React.useState<SurveyPageEditViewModel[]>(props.pages);
  React.useEffect(() => {
    setPages(props.pages);
  }, [props.pages]);


  return (
    <>
      {pages.map((page) => {
        return <SurveyPageView key={page.id} page={page} {...props} />;
      })}
    </>
  );
};
