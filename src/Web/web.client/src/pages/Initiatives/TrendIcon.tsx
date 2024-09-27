import React from 'react';

export const TrendIcon: React.FC<{ percentIncrease: number }> = (props) => {

  return (
    <div>
      {props.percentIncrease > 0 ?
        <img src='../img/icons/arrow-green.svg' alt={props.percentIncrease.toFixed(2).toString()}></img>
        :
        <>
          {props.percentIncrease === 0 ?
            <p>--</p>
            :
            <img src='../img/icons/arrow-down.svg' alt={props.percentIncrease.toFixed(2).toString()}></img>
          }
        </>
      }
    </div >
  );
};
