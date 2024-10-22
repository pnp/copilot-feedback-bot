
import { Gauge, gaugeClasses } from '@mui/x-charts';
import React from 'react';

export const NumberGauge: React.FC<{ val: number }> = (props) => {

    return (
        <Gauge startAngle={-90} endAngle={90} value={props.val} height={80}
            sx={() => ({
                [`& .${gaugeClasses.valueArc}`]: {
                    fill: '#ac8400',
                }
            })}
        />
    );
};
