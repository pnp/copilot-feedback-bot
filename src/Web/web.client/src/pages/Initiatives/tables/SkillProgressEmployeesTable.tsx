import React from 'react';
import { PercentIcon } from '../PercentIcon';
import { Dialog, DialogContent } from '@mui/material';

interface Props {
  employeeContributionBreakdown: EmployeeProgressForSkill[],
  skillName: SkillName,
  openFilterModal: boolean
  handleClose: Function
}

export const SkillProgressEmployeesTable: React.FC<Props> = (props) => {

  const handleClose = () => props.handleClose();

  return (
    <>
      <Dialog
        open={props.openFilterModal}
        onClose={handleClose}
        aria-labelledby="modal-modal-title"
        aria-describedby="modal-modal-description"
      >
        {props.employeeContributionBreakdown.length > 0 ?
          <DialogContent dividers={true}>

            <table className='table'>
              <thead>
                <tr>
                  <th>Employee ID</th>
                  <th>Av Level Start</th>
                  <th>Av Level End</th>
                  <th>% Increase</th>
                </tr>
              </thead>
              <tbody>
                {props.employeeContributionBreakdown.map(a => {
                  return <tr key={a.employeeId}>
                    <td>{a.employeeId}</td>
                    <td>{a.averageLevelStart}</td>
                    <td>{a.averageLevelEnd}</td>
                    <td><PercentIcon percent={a.percentageIncrease} /></td>
                  </tr>
                })}
              </tbody>
            </table>
          </DialogContent>
          :
          <div>No employees found for skill</div>
        }
      </Dialog>
    </>
  );
};
