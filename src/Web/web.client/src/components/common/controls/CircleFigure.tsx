
import React from 'react';
import { CircularProgressbar, buildStyles } from 'react-circular-progressbar';
import 'react-circular-progressbar/dist/styles.css';

export const CircleFigure: React.FC<{ val: number }> = (props) => {

  return (
    <div style={{ width: 120, height: 120 }}>
      <CircularProgressbar value={props.val} text={`${props.val}%`} styles={buildStyles({

        // Whether to use rounded or flat corners on the ends - can use 'butt' or 'round'
        strokeLinecap: 'butt',

        // How long animation takes to go from one percentage to another, in seconds
        pathTransitionDuration: 0.5,

        // Colors
        textColor: '#ac8400',
        pathColor: `#ac8400`,
        trailColor: '#F1F4FA',
      })} />
    </div>
  );
};
