import React from 'react';
import { NumberLabel } from '../../components/common/controls/NumberLabel';
import { DateOnlyLabel } from '../../components/common/controls/DateOnlyLabel';
import { MoneyLabel } from '../../components/common/controls/MoneyLabel';
import { SkillProgressTable } from './tables/SkillProgressTable';
import { SkillProgressEmployeesTable } from './tables/SkillProgressEmployeesTable';
import { SkillsGauge } from '../../components/common/controls/SkillsGauge';

export const InitiativesDetails: React.FC<{ token: string, client: Client, skillsInitiativeReport: SkillsInitiativeReport }> = (props) => {

  const [selectedSkillProgressDetail, setSelectedSkillProgressDetail] = React.useState<SkillProgress | null>(null);

  //const handleOpen = () => setSelectedSkillProgressDetail(true);
  const handleClose = () => setSelectedSkillProgressDetail(null);

  return (
    <div>
      <h1>Skills Initiative - {props.skillsInitiativeReport.skillsInitiative.name}</h1>
      <section className="dashboard--summary smallpadbottom">
        <div className="col-wrap key-stats">

          <div className="col-03">
            <div className="box-wrap">
              <div className="stats-wrap">
                <h3>Start</h3>
                <h2><DateOnlyLabel val={props.skillsInitiativeReport.skillsInitiative.start} /></h2>
              </div>
            </div>
          </div>

          <div className="col-03">
            <div className="box-wrap">
              <div className="stats-wrap">
                <h3>End</h3>
                <h2><DateOnlyLabel val={props.skillsInitiativeReport.skillsInitiative.end} /></h2>
              </div>
            </div>
          </div>

          <div className="col-03">
            <div className="box-wrap">
              <div className="stats-wrap">
                <h3><NumberLabel val={props.skillsInitiativeReport.totalPercentComplete} decimalPlaces={2} />% Complete</h3>
                <SkillsGauge val={props.skillsInitiativeReport.totalPercentComplete}
                />
              </div>
            </div>
          </div>


          <div className="col-03">
            <div className="box-wrap">
              <div className="stats-wrap">

                <h3>Investment</h3>
                <h2><MoneyLabel val={props.skillsInitiativeReport.skillsInitiative.cost} /></h2>
              </div>
            </div>
          </div>

          <div className="col-03">
            <div className="box-wrap">
              <div className="stats-wrap">
                <h3>Return as of Today</h3>
                <h2><MoneyLabel val={props.skillsInitiativeReport.roi} /></h2>
              </div>
            </div>
          </div>

          <div className="col-03">
            <div className="box-wrap icon">
              {props.skillsInitiativeReport.percentIncreaseFromStart > 1 ?
                <div className="icon-wrap reports-up"></div>
                :
                <div className="icon-wrap reports-down"></div>
              }
              <div className="stats-wrap">
                <h2><NumberLabel val={props.skillsInitiativeReport.percentIncreaseFromStart} decimalPlaces={2} />%</h2>
                <p><strong>Change from Start</strong></p>
              </div>
            </div>
          </div>

        </div>
      </section>

      {selectedSkillProgressDetail ?
        <>
          <h3>Details for {selectedSkillProgressDetail.skillName.name}</h3>
          <SkillProgressEmployeesTable employeeContributionBreakdown={selectedSkillProgressDetail.employeeContributionBreakdown}
            skillName={selectedSkillProgressDetail.skillName} handleClose={handleClose} openFilterModal={selectedSkillProgressDetail !== null} />
        </>
        :
        <>
          <h3>For Cohort</h3>
          <table className='table'>
            <thead>
              <tr>
                <th>Job Title</th>
              </tr>
            </thead>
            <tbody>
              {props.skillsInitiativeReport.skillsInitiative.targetAudiences.map(a => {
                return <tr key={a.id}>
                  <td>{a.jobTitle.name}</td>
                </tr>
              })}
            </tbody>
          </table>
          <h3>Progress</h3>
          <SkillProgressTable skillProgress={props.skillsInitiativeReport.skillProgress}
            progressItemSelected={setSelectedSkillProgressDetail} />
        </>
      }

    </div >
  );
};
