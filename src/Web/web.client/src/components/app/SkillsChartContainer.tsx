import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    BarElement,
    LineController,
    ArcElement,
    Filler
  } from 'chart.js';
import { PropsWithChildren } from 'react';

  ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    LineController,
    ArcElement,
    Filler
  );

export const SkillsChartContainer: React.FC<PropsWithChildren<{}>> = (props) => {

    return <>{props.children}</>;
}
