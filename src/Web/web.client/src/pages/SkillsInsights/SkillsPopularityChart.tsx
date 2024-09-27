
import { Doughnut } from 'react-chartjs-2';

import React from 'react';
import { getGraphDataFromSkillsNameStats } from '../../utils/GraphJsConverters';
import { ChartData } from 'chart.js';

export const SkillsPopularityChart: React.FC<{ data: SkillsNameStats }> = (props) => {

  const [sentimentChannelCount, setSentimentChannelCountData] = React.useState<ChartData<"doughnut", number[], string> | null>(null);
  React.useEffect(() => {
    setSentimentChannelCountData(getGraphDataFromSkillsNameStats(props.data));
  }, [props.data]);

  const depoSentimentOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: false,
      },
    },
    maintainAspectRatio: false
  };

  return (
    <div>

      {sentimentChannelCount &&
        <>
          <Doughnut options={depoSentimentOptions} data={sentimentChannelCount} width={"1000px"} height={"500px"} />
        </>
      }

    </div>
  );
};

