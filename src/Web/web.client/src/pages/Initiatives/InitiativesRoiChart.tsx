import { Pie } from 'react-chartjs-2';

import React from 'react';
import { ColourPicker } from '../../utils/ColourPicker';

export const InitiativesRoiChart: React.FC<{ totalValue: number, initiativesRoi: number }> = (props) => {

  const data = {
    labels: ['I', 'T'],
    datasets: [
      {
        data: [props.initiativesRoi, props.totalValue],
        backgroundColor: [
          '#000000',
          ColourPicker.chartColours[0]
        ],
        borderColor: [
          '#000000',
        ],
        borderWidth: 0,

      },
    ],
  };
  return (
    <div>

      <Pie data={data} options={ { plugins:{ legend: {display: false }}, maintainAspectRatio: false}} />

    </div>
  );
};

