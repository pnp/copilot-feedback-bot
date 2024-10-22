
import React from 'react';
import { NumberLabel } from './NumberLabel';

export const MoneyLabel: React.FC<{ val?: number }> = (props) => {

  return (
    <>
      {props.val ?
        <>
          $<NumberLabel val={props.val} decimalPlaces={0} /></>
        :
        <>$-</>
      }
    </>
  );
};
