
import React from 'react';

export const TrendLabel: React.FC<{ val: number, label: string, classNameOverride?: string }> = (props) => {

  return (
    <span>
      {props.val < 0 &&
        <h4 className={props.classNameOverride ?? "negative"}>{props.label}: {props.val.toFixed(2)}%</h4>
      }
      {props.val === 0 &&
        <h4 className={props.classNameOverride ?? ""}>{props.label}: {props.val.toFixed(2)}%</h4>
      }
      {props.val > 0 &&
        <h4 className={props.classNameOverride ?? "positive"}>{props.label}: +{props.val.toFixed(2)}%</h4>
      }
    </span>
  );
};
