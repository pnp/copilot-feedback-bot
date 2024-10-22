
import React from 'react';

export const NumberLabel: React.FC<{ val?: number, decimalPlaces: number }> = (props) => {

  return (
    <>
      {props.val ?
        <>
          {Number(props.val.toFixed(props.decimalPlaces)).toLocaleString()}</>
        :
        <></>
      }
    </>
  );
};
