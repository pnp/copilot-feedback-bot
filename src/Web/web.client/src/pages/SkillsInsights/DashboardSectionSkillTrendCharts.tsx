import React from 'react';
import { SkillsMiniChart } from '../Dashboard/SkillsMiniChart';
import { ChartView } from '../../apimodels/Enums';

export const DashboardSectionSkillTrendCharts: React.FC<{ skillsDeviation: SkillsDeviationModel | null | undefined, datesCovered?: Date[], chartRange: ChartView }> = (props) => {

  return (
    <>
      <div className="stats-wrap">
        <h3>Emerging Skills</h3>
        {props.skillsDeviation && props.datesCovered &&
          <SkillsMiniChart data={props.skillsDeviation.skillsGoingUp} datesCovered={props.datesCovered} chartRange={props.chartRange} />
        }
      </div>


      <div className="stats-wrap">
        <h3>Stale Skills</h3>

        {props.skillsDeviation && props.datesCovered &&
          <SkillsMiniChart data={props.skillsDeviation.flatLineSkills} datesCovered={props.datesCovered} chartRange={props.chartRange} />
        }
      </div>

      <div className="stats-wrap">
        <h3>Most Valuable Skills</h3>
        {props.skillsDeviation && props.datesCovered &&
          <SkillsMiniChart data={props.skillsDeviation.topValuedSkills} datesCovered={props.datesCovered} chartRange={props.chartRange} />
        }
      </div>
    </>
  );
};
