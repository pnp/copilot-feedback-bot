import { Bar } from 'react-chartjs-2';

import React from 'react';
import { ChartData } from 'chart.js';
import { EntityWithScore } from '../../apimodels/Models';

export const LookupLeagueChart: React.FC<{ propName: string, data: EntityWithScore<string>[] }> = (props) => {

  const [chartData, setChartData] = React.useState<ChartData<"bar", number[], string>>({ datasets: [], labels: [] });

  React.useEffect(() => {

    const d: ChartData<"bar", number[], string> =
    {
      datasets: [],
      labels: props.data.map(d => d.entity),
    };

    d.datasets.push({
      label: props.propName,
      type: 'bar' as const,
      data: props.data.map(s => s.score),
      borderColor: "#002050",
      backgroundColor: "#002050",
    });
    setChartData(d);
  }, [props.data, props.propName]);

  const chartOptions = {
    responsive: true,
    indexAxis: 'y' as const,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: false,
      },
    },
    maintainAspectRatio: true
  };

  return (
    <div>

      {chartData &&
        <>
          <Bar options={chartOptions} data={chartData} width={"700px"} height={"500px"} />

        </>
      }

    </div>
  );
};

