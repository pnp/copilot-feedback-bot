import React from 'react';
import { NumberLabel } from '../../../components/common/controls/NumberLabel';
import { PercentIcon } from '../PercentIcon';
import { TrendIcon } from '../TrendIcon';
import { Button } from '@mui/material';

export const SkillProgressTable: React.FC<{ skillProgress: SkillProgress[], progressItemSelected: Function }> = (props) => {

  const select = React.useCallback((s: SkillProgress) => {

    props.progressItemSelected(s);
  }, [props]);

  return (

    <table className='table'>
      <thead>
        <tr>
          <th>Skill</th>
          <th>Initial Total</th>
          <th>Current Total</th>
          <th>Target Av. Level</th>
          <th colSpan={2}>Trend</th>
          <th># of Employees</th>
        </tr>
      </thead>
      <tbody>
        {props.skillProgress.map(p => {
          return <tr key={p.skillName.id}>
            <td>{p.skillName.name}</td>
            <td>$<NumberLabel val={p.initialTotalCashValue} decimalPlaces={2} /> (average: <NumberLabel val={p.initialAverageValue} decimalPlaces={2} />)</td>
            <td>$<NumberLabel val={p.currentTotalCashLevel} decimalPlaces={2} /> (average: <NumberLabel val={p.currentAverageLevel} decimalPlaces={2} />)</td>
            <td><NumberLabel val={p.targetAverageLevel} decimalPlaces={2} /></td>

            <td>
              <TrendIcon percentIncrease={p.percentIncreaseFromStart} />
            </td>
            <td>
              <PercentIcon percent={p.percentCompleteToTarget} />
            </td>
            <td><Button onClick={() => select(p)}>{p.employeeContributionBreakdown.length} employees</Button></td>
          </tr>
        })}
      </tbody>
    </table>

  );
};
