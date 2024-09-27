
import { Line } from 'react-chartjs-2';

import React from 'react';
import { ChartData } from 'chart.js';
import { ColourPicker } from '../../../utils/ColourPicker';
import moment from 'moment';
import { getTotalValue } from '../../../utils/DataUtils';

export const SkillsTrendsChart: React.FC<{ data: SkillsCube, showActive: boolean }> = (props) => {

  const [skillsData, setSkillsData] = React.useState<ChartData<"line", number[], string> | null>(null);

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Skills rating trends',
      }
    },
    scales: {
      y: {
        ticks: {
          callback: (value: any) => {
            return '$' + value.toLocaleString();
          }
        },
        min: 0
      }
    },
    maintainAspectRatio: true
  };

  const getLineGraphDataFromSkillsCube = React.useCallback((model: SkillsCube): ChartData<"line", number[], string> => {

    if (model.demographicStats.length === 0) {
      return {
        datasets: [],
        labels: []
      };
    }

    const colours = new ColourPicker(0);
    const d: ChartData<'line', number[], string> =
    {
      datasets: [],
      labels: model.metadata.datesCovered.map(d => moment(d).format('yyyy-MM-DD'))
    };

    // Data is in format $demographic > $date > $score
    let datesAverageDic = new Map<Date, number[]>();
    model.demographicStats.forEach((demographicStat: DemographicDatespanStatistics) => {

      let statsToShow: ValueForDateSpan[];
      if (props.showActive) {
        statsToShow = demographicStat.activeStats;
      }
      else
        statsToShow = demographicStat.inactiveStats;

      if (getTotalValue(statsToShow) > 0) {

        // Build averages for all demographics
        statsToShow.forEach(ds => {
          const existing = datesAverageDic.get(ds.from) ?? [];
          existing.push(ds.skillValueOnDate.value);
          datesAverageDic.set(ds.from, existing);
        });

        const col = colours.charColour();
        d.datasets.push({
          label: demographicStat.demographicName,
          type: 'line' as const,
          data: statsToShow.map((s) => s.skillValueOnDate.value),   // Map all score values as array
          borderColor: col,
          backgroundColor: col,
          order: 2
        });
      }
    });
    return d;
  }, [props.showActive]);

  React.useEffect(() => {
    setSkillsData(getLineGraphDataFromSkillsCube(props.data));
  }, [props.data, getLineGraphDataFromSkillsCube]);


  return (
    <div>

      {skillsData &&
        <>
          <Line options={options} data={skillsData} width={"1000px"} height={"500px"} />
        </>
      }

    </div>
  );
};
