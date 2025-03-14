import { Pie } from 'react-chartjs-2';

import React from 'react';
import { ColourPicker } from '../../utils/ColourPicker';
import { SurveysReport } from '../../apimodels/Models';

export const AnswersPieChart: React.FC<{stats: SurveysReport}> = (props) => {

  const data = {
    labels: ['Positive Answer', 'Negative Answer'],
    datasets: [
      {
        data: [props.stats.percentageOfAnswersWithPositiveResult, 100 - props.stats.percentageOfAnswersWithPositiveResult],
        backgroundColor: [
            ColourPicker.chartColours[0],
            ColourPicker.chartColours[1]
        ],
        borderColor: [
          '#002050',
        ],
        borderWidth: 1,
      }
    ],
    plugins: {
      legend: {
          display: true,
      }
  }
  };
  return (
    <div>

      <Pie data={data} options={ { plugins:{ legend: {display: false }}, maintainAspectRatio: false}} />

    </div>
  );
};

