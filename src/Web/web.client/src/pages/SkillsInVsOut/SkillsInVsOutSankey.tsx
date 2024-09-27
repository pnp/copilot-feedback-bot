import React, { useCallback } from 'react';
import { ChartData } from 'chart.js';
import moment from 'moment';
import { ChartView } from '../../apimodels/Enums';
import { Chart as ChartJS } from "chart.js";
import { SankeyController, Flow } from 'chartjs-chart-sankey';
import { Chart } from "react-chartjs-2";

ChartJS.register(SankeyController, Flow);

interface Props {
  skillsData: SkillsCube,
  loading: boolean,
  skillsIdFilter?: string,
  chartRange: ChartView
}

interface SankeyData { from: string, to: string, flow: number }

export const SkillsInVsOutSankey: React.FC<Props> = (props) => {
  const [chartData, setChartData] = React.useState<SankeyData[]>();

  const data: ChartData<"sankey", SankeyData[], string> = {
    datasets: [
      {
        label: "My sankey",
        data: chartData ?? [],
        colorMode: "gradient",
        colorFrom: () => "#323232",
        colorTo: () => "#C8AC72",
        size: "max"
      }
    ]
  };
  const options = {
    plugins: {
      legend: {
        display: false
      }
    }
  };


  const getLabel = useCallback((date: Date): string => props.chartRange === ChartView.OneWeek ||
    props.chartRange === ChartView.OneMonth ? moment(date).format('MMM D') : moment(date).format('MMMM YYYY'), [props.chartRange]);

  const getSankeyData = useCallback((model: SkillsCube): SankeyData[] => {

    const d: SankeyData[] = [];
    let lastInactiveSkillsValue = 0;

    // Data is in format $demographic > $date > $total
    for (let index = 0; index < model.metadata.datesCovered.length; index++) {
      const dateCovered = model.metadata.datesCovered[index];

      if (index + 1 < model.metadata.datesCovered.length) {
        const nextDateCovered = model.metadata.datesCovered[index + 1];

        // Find out total value between dates
        let totalValueForDate = 0, totalInactiveForDate = 0;

        // One demographic. Just take 1st one
        if (model.demographicStats.length > 0) {

          const demographic = model.demographicStats[0];

          // Process default demographic only

          demographic.activeStats.forEach(stat => {

            // Convert to numbers and compare
            if (moment(stat.from).toDate().getTime() === moment(dateCovered).toDate().getTime()) {
              totalValueForDate += stat.skillValueOnDate.value;
            }
          });

          demographic.inactiveStats.forEach(stat => {

            // We only care about "new" lost skills, as all filter to a single sankey endpoint
            if (moment(stat.from).toDate().getTime() === moment(dateCovered).toDate().getTime()) {
              totalInactiveForDate += stat.skillValueOnDate.value;
            }
          });
        }

        d.push({ from: getLabel(dateCovered), to: getLabel(nextDateCovered), flow: totalValueForDate });

        // For "lost skills" we don't maintain a value for each date (like a line-chart), unless it grows each date.
        // That's because lost skills goes to a single "lost" endpoint in the sankey, so only send higher lost skill values
        let inactiveVal = totalInactiveForDate - lastInactiveSkillsValue;
        if (inactiveVal > 0) {
          d.push({ from: getLabel(dateCovered), to: "Skills lost", flow: inactiveVal });
          if (lastInactiveSkillsValue < totalInactiveForDate) {
            lastInactiveSkillsValue = totalInactiveForDate;
          }
        }
      }
    }

    return d;
  }, [getLabel]);

  React.useEffect(() => {
    setChartData(getSankeyData(props.skillsData));
  }, [props.skillsData, getSankeyData]);

  return (
    <div>
      <Chart type="sankey" data={data} options={options} />
    </div>
  );

};
