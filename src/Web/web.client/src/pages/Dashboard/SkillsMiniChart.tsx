import { Line } from 'react-chartjs-2';

import React from 'react';
import { ChartData } from 'chart.js';
import { ColourPicker } from '../../utils/ColourPicker';
import moment from 'moment';
import { ChartView } from '../../apimodels/Enums';

export const SkillsMiniChart: React.FC<{ data: DemographicDatespanStatistics[], datesCovered: Date[], chartRange: ChartView }> = (props) => {

  const [chartData, setChartData] = React.useState<ChartData<"line", number[], Date> | null>(null);

  React.useEffect(() => {

    const colours = new ColourPicker(0);

    if (props.data) {
      const d: ChartData<"line", number[], Date> =
      {
        datasets: [],
        labels: props.datesCovered.map(d => moment(d).toDate())
      };

      // Data is in format $demographic > $date > $total
      props.data.forEach((demographicStat: DemographicDatespanStatistics) => {

        const col = colours.charColour();
        d.datasets.push({
          label: demographicStat.demographicName,
          type: 'line' as const,
          data: demographicStat.activeStats.map((s) => s.skillValueOnDate.value),
          borderColor: col,
          backgroundColor: col,
        });
      });

      setChartData(d);
    }

  }, [props.data, props.datesCovered]);

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
        display: true
      },
      title: {
        display: false,
      }
    },
    scales: {
      y: {
        ticks: {
          callback: (value: any) => {
            return '$' + value.toLocaleString();
          }
        },
      },
      x: xScale as any
    },
    maintainAspectRatio: false
  };

  return (
    <div>

      {chartData &&
        <>
          <Line options={options} data={chartData} width={400} height={200} />
        </>
      }

    </div>
  );
};

