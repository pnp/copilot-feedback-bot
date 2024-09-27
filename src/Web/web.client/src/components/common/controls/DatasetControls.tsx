import React from 'react';
import { ChartView } from '../../../apimodels/Enums';
import { Button } from '@mui/material';

interface Props {
  newChartViewRange: Function,
  filterOptionClicked?: Function,
  loading: boolean,
  chartRange: ChartView
}

export const DatasetControls: React.FC<Props> = (props) => {

  const btnDurationOneWeek = props.chartRange === ChartView.OneWeek ? "btn" : "btn light";
  const btnDurationOneMonth = props.chartRange === ChartView.OneMonth ? "btn" : "btn light";
  const btnDurationThreeMonths = props.chartRange === ChartView.ThreeMonths ? "btn" : "btn light";
  const btnDurationSixMonths = props.chartRange === ChartView.SixMonths ? "btn" : "btn light";
  const btnDurationOneYear = props.chartRange === ChartView.OneYear ? "btn" : "btn light";

  const setDurationOneWeek = () => {
    loadClientOverviewTrends(ChartView.OneWeek);
  }
  const setDurationOneMonth = () => {
    loadClientOverviewTrends(ChartView.OneMonth);
  }
  const setDurationThreeMonths = () => {
    loadClientOverviewTrends(ChartView.ThreeMonths);
  }
  const setDurationSixMonths = () => {
    loadClientOverviewTrends(ChartView.SixMonths);
  }
  const setDurationOneYear = () => {
    loadClientOverviewTrends(ChartView.OneYear);
  }

  const loadClientOverviewTrends = (length: ChartView) => {
    props.newChartViewRange(length);
  }

  return (
    <div>

      <button onClick={setDurationOneWeek} className={btnDurationOneWeek} disabled={props.loading}>1 Week</button>
      <button onClick={setDurationOneMonth} className={btnDurationOneMonth} disabled={props.loading}>1 Month</button>
      <button onClick={setDurationThreeMonths} className={btnDurationThreeMonths} disabled={props.loading}>3 Months</button>
      <button onClick={setDurationSixMonths} className={btnDurationSixMonths} disabled={props.loading}>6 Months</button>
      <button onClick={setDurationOneYear} className={btnDurationOneYear} disabled={props.loading}>1 Year</button>

      {props.filterOptionClicked &&
        <Button onClick={() => props.filterOptionClicked!()}>Pick Filters</Button>
      }
    </div>
  );
};
