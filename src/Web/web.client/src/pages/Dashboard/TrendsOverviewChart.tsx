import { Line } from 'react-chartjs-2';
import React from 'react';
import { ChartData } from 'chart.js';
import moment from 'moment';
import { ChartView } from '../../apimodels/Enums';
import { TimeScale, Chart } from "chart.js";

Chart.register(TimeScale);

interface Props {
  skillsData: SkillsCube,
  loading: boolean,
  skillsIdFilter?: string,
  chartRange: ChartView
}

export const TrendsOverviewChart: React.FC<Props> = (props) => {

  const [chartData, setChartData] = React.useState<ChartData<"line", number[], Date>>();

  React.useEffect(() => {
    setChartData(getGraphDataFromSkillsDataByJobTitle(props.skillsData));
  }, [props.skillsData]);


  const xScale = props.chartRange === ChartView.OneWeek ||props.chartRange === ChartView.OneMonth ?
    {
      type: 'time',
      time: {
        unit: 'day'
      }
    } : {
      type: 'time',
      time: {
        unit: 'month'
      }
    };
  const options = {
    responsive: true,
    plugins: {
      legend: {
        display: true,

      },
      title: {
        display: false,
      }
    },
    scales: {
      y1: {
        ticks: {
          callback: (value: any) => {
            return '$' + value.toLocaleString();
          }
        },
        min: 0
      },
      y2: {
        display: false,
        min: 0,
        max: 100
      },
      x: xScale as any
    },
    maintainAspectRatio: false
  };

  const getGraphDataFromSkillsDataByJobTitle = (model: SkillsCube): ChartData<"line", number[], Date> => {

    if (model.demographicStats.length === 0) {
      return {
        datasets: [],
        labels: []
      };
    }

    const d: ChartData<"line", number[], Date> =
    {
      datasets: [],
      labels: model.metadata.datesCovered.map(d => moment(d).toDate())
    };

    // Data is in format $demographic > $date > $total
    model.demographicStats.forEach((demographicStat: DemographicDatespanStatistics) => {

      d.datasets.push({
        label: "Total value",
        type: 'line' as const,
        data: demographicStat.activeStats.map((s) => s.skillValueOnDate.value),
        borderColor: "#AC8400",
        backgroundColor: "#C8AC72",
        fill: true,
        order: 2,
        yAxisID: 'y1'
      });

      d.datasets.push({
        label: "Confidence",
        type: 'line' as const,
        data: demographicStat.activeStats.map((s) => s.skillValueOnDate.confidence),
        borderColor: "#323232",
        order: 1,
        yAxisID: 'y2'
      });
    });



    return d;
  }

  return (
    <div>

      {chartData &&
        <>
          <Line options={options} data={chartData} width={"800px"} height={"300px"} />
        </>
      }

    </div>
  );
};
