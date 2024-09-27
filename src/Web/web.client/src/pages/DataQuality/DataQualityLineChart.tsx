import { Line } from 'react-chartjs-2';
import React from 'react';
import { ChartData } from 'chart.js';
import { ChartView } from '../../apimodels/Enums';
import { TimeScale, Chart } from "chart.js";
import { faker } from '@faker-js/faker'
import { ColourPicker } from '../../utils/ColourPicker';

Chart.register(TimeScale);

interface Props {
  loading: boolean,
  chartRange: ChartView
}

export const DataQualityLineChart: React.FC<Props> = () => {

  const [chartData, setChartData] = React.useState<ChartData<"line", number[], any>>();

  React.useEffect(() => {
    setChartData(getGraphDataFromSkillsDataByJobTitle());
  }, []);

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
    },
    scales: {
      y: {
        display: true,
        min: 0,
        max: 100
      },
    },
    maintainAspectRatio: false
  };

  const getGraphDataFromSkillsDataByJobTitle = (): ChartData<"line", number[], string> => {

    const labels = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];

    const data: ChartData<"line", number[], string> = {
      labels,
      datasets: [
        {
          label: 'Precision',
          data: labels.map(() => faker.datatype.number({ min: 60, max: 100 })),
          borderColor: ColourPicker.chartColours[0],
          backgroundColor: ColourPicker.chartColours[0],
        },
        {
          label: 'Validity',
          data: labels.map(() => faker.datatype.number({ min: 40, max: 90 })),
          borderColor: ColourPicker.chartColours[1],
          backgroundColor: ColourPicker.chartColours[1],
        },
        {
          label: 'Completeness',
          data: labels.map(() => faker.datatype.number({ min: 10, max: 40 })),
          borderColor: ColourPicker.chartColours[2],
          backgroundColor: ColourPicker.chartColours[2],
        },
        {
          label: 'Consistency',
          data: labels.map(() => faker.datatype.number({ min: 90, max: 100 })),
          borderColor: ColourPicker.chartColours[3],
          backgroundColor: ColourPicker.chartColours[3],
        },
        {
          label: 'Timeliness',
          data: labels.map(() => faker.datatype.number({ min: 90, max: 100 })),
          borderColor: ColourPicker.chartColours[4],
          backgroundColor: ColourPicker.chartColours[4],
        },
        {
          label: 'Accuracy',
          data: labels.map(() => faker.datatype.number({ min: 60, max: 90 })),
            borderColor: ColourPicker.chartColours[5],
            backgroundColor: ColourPicker.chartColours[5],
        },
      ],
    };

    return data;
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
