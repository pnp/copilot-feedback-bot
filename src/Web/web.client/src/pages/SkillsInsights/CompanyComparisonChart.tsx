import { Bar } from 'react-chartjs-2';

import React from 'react';
import { ChartData } from 'chart.js';
import { getGraphDataFromCognitiveStatsCompanyComparisonModel } from '../../utils/GraphJsConverters';

export const CompanyComparisonChart: React.FC<{ data: SkillsCompanyComparisonModel }> = (props) => {

  const [companyComparisonData, setCompanyComparisonData] = React.useState<ChartData<"bar", number[], string>>({ datasets: [], labels: [] });

  React.useEffect(() => {
    setCompanyComparisonData(getGraphDataFromCognitiveStatsCompanyComparisonModel(props.data));
  }, [props.data]);


  const companyScoreComparisonOptions = {
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

      {companyComparisonData &&
        <>
          <Bar options={companyScoreComparisonOptions} data={companyComparisonData} width={"1000px"} height={"500px"} />

        </>
      }

    </div>
  );
};

