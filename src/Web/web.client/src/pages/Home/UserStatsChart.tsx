import { Pie } from 'react-chartjs-2';

import React from 'react';
import { ColourPicker } from '../../utils/ColourPicker';
import { BasicStats } from '../../apimodels/Models';

export const UserStatsChart: React.FC<{stats: BasicStats}> = (props) => {

  const data = {
    labels: ['Users surveyed', 'Users discovered'],
    datasets: [
      {
        data: [props.stats.usersSurveyed, props.stats.usersFound],
        backgroundColor: [
            ColourPicker.chartColours[2],
            ColourPicker.chartColours[3],
        ],
        borderColor: [
          '#002050',
        ],
        borderWidth: 1,

      },
    ],
  };
  return (
    <div>

      <Pie data={data} options={ { plugins:{ legend: {display: false }}, maintainAspectRatio: false}} />

    </div>
  );
};

