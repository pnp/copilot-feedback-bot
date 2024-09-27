import React from 'react';
import { CircularProgressbar, buildStyles } from 'react-circular-progressbar';
import 'react-circular-progressbar/dist/styles.css';

export const PercentIcon: React.FC<{ percent: number }> = (props) => {

  const circularProgressStyle =
    buildStyles({
      // How long animation takes to go from one percentage to another, in seconds
      pathTransitionDuration: 0.5,
      textSize: 24,
      pathColor: "#C8AC72",
      textColor: '#000000',
      trailColor: '#d6d6d6',
    });

  return (
    <div style={{ width: 50, height: 50 }}>
      <CircularProgressbar value={props.percent} text={`${props.percent.toFixed(0)}%`}
        styles={circularProgressStyle} />
    </div>
  );
};
