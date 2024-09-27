
import moment from 'moment';
import React from 'react';

export const DateOnlyLabel: React.FC<{ val?: Date }> = (props) => {

  return (
    <>
      {props.val ?
        <>
          {moment(props.val).format('DD-MM-yyyy')}</>
        :
        <>--</>
      }
    </>
  );
};
