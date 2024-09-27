import { ChartTypeRegistry } from 'chart.js';
import { Chart } from 'react-chartjs-2';

import React from 'react';
import { ChartData } from 'chart.js';
import { ColourPicker } from '../../../utils/ColourPicker';
import moment from 'moment';
import { avg, getTotalValue } from "../../../utils/DataUtils";

export const SkillsByJobTitleChart: React.FC<{ data: SkillsCube, showActive: boolean }> = (props) => {

  const [skillsData, setSkillsData] = React.useState<ChartData<keyof ChartTypeRegistry, number[], string> | null>(null);

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Skills rating by job title',
      }
    },
    scales: {
      y: {
        ticks: {
          callback: (value: any) => {
            return '$' + value.toLocaleString();
          }
        }
      }
    },
    maintainAspectRatio: true
  };

  // Convert models into Graph data objects
  const getGraphDataFromSkillsDataByJobTitle = React.useCallback((model: SkillsCube): ChartData<keyof ChartTypeRegistry, number[], string> => {

    if (model.demographicStats.length === 0) {
      return {
        datasets: [],
        labels: []
      };
    }

    const colours = new ColourPicker(0);
    const d: ChartData<keyof ChartTypeRegistry, number[], string> =
    {
      datasets: [],
      labels: model.metadata.datesCovered.map(d => moment(d).format('yyyy-MM-DD'))
    };

    // Data is in format $demographic > $date > $sentimentScore
    let datesAverageDic = new Map<Date, number[]>();

    model.demographicStats.forEach((demographicStat: DemographicDatespanStatistics) => {
      let statsToShow: ValueForDateSpan[];

      if (props.showActive)
        statsToShow = demographicStat.activeStats;
      else
        statsToShow = demographicStat.inactiveStats;

      // Don't include demographics with literally zero value in scope
      if (getTotalValue(statsToShow) > 0) {

        // Build averages for all demographics
        statsToShow.forEach((ds: ValueForDateSpan) => {
          const existing = datesAverageDic.get(ds.from) ?? [];
          existing.push(ds.skillValueOnDate.value);
          datesAverageDic.set(ds.from, existing);
        });

        const colour: string = colours.charColour();

        d.datasets.push({
          label: demographicStat.demographicName,
          type: 'bar' as const,
          data: statsToShow.map((s) => s.skillValueOnDate.value),   // Map all sentiment values as array
          borderColor: colour,
          backgroundColor: colour,
          order: 2
        });
      }

    });

    // Calculate averages & push single dataset
    const averageVals: number[] = [];
    datesAverageDic.forEach((vals: number[]) => {
      averageVals.push(avg(vals));
    });
    d.datasets.push({
      label: "Average",
      type: 'line' as const,
      data: averageVals,
      borderColor: "#AC8400",
      backgroundColor: "#C8AC72",
      order: 1
    });

    return d;
  }, [props.showActive]);

  React.useEffect(() => {
    setSkillsData(getGraphDataFromSkillsDataByJobTitle(props.data));
  }, [props.data, getGraphDataFromSkillsDataByJobTitle]);


  return (
    <div>

      {skillsData &&
        <>
          <Chart type='bar' options={options} data={skillsData} width={"1000px"} height={"500px"} />
        </>
      }

    </div>
  );
};

