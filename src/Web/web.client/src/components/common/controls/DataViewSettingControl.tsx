import React from 'react';
import { DataViewSetting } from '../../../apimodels/Enums';

interface Props {
  newViewRange?: Function,
  loading: boolean,
  setting: DataViewSetting
}

export const DataViewSettingControl: React.FC<Props> = (props) => {

  const btnFinancial = props.setting === DataViewSetting.Financial ? "btn" : "btn light";
  const btnRubric = props.setting === DataViewSetting.Rubric ? "btn" : "btn light";

  const setViewFinancial = () => {
    setView(DataViewSetting.Financial);
  }
  const setViewRubric = () => {
    setView(DataViewSetting.Rubric);
  }

  const setView = (v: DataViewSetting) => {
    if (props.newViewRange)
      props.newViewRange(v);
  }

  return (
    <div style={{marginBottom: 10}}>
      <button onClick={setViewFinancial} className={btnFinancial} disabled={props.loading}>Financial</button>
      <button onClick={setViewRubric} className={btnRubric} disabled={props.loading}>Rubric</button>
    </div>
  );
};
