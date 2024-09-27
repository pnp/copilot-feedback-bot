import React from 'react';
import { deleteSkillsInitiative, getSkillsInitiatives } from '../../api/ApiCalls';
import { InitiativesDetails } from './InitiativesDetails';
import { LoadingSpinner } from '../../components/common/controls/LoadingSpinner';
import { PercentIcon } from './PercentIcon';
import { TrendIcon } from './TrendIcon';
import { MoneyLabel } from '../../components/common/controls/MoneyLabel';
import { InitiativesAddEdit } from './InitiativesAddEdit';
import { Grid } from '@mui/material';
import { InitiativesRoiChart } from './InitiativesRoiChart';
import { ColourPicker } from '../../utils/ColourPicker';

enum View {
  List,
  Details,
  AddEdit
}

export const InitiativesPage: React.FC<{ token: string, client: Client }> = (props) => {

  const [viewMode, setViewMode] = React.useState<View>(View.List);

  const [selectedReport, setSelectedReport] = React.useState<SkillsInitiativeReport | null>(null);
  const [skillsData, setSkillsData] = React.useState<InitiativesSummary>();
  const [totalRoi, setTotalRoi] = React.useState<number>(0);


  const selectReport = React.useCallback((r: SkillsInitiativeReport) => {
    setSelectedReport(r);
    setViewMode(View.Details);
  }, []);

  const refreshData = React.useCallback(() => {

    // Demograph stats
    getSkillsInitiatives(props.token)
      .then(async response => {
        setSkillsData(response);

        let totalRoi: number = 0;
        response.reports.forEach(r => totalRoi += r.roi);
        setTotalRoi(totalRoi);
      });

    window.initMainJs();
  }, [props.token]);

  React.useEffect(() => {
    refreshData();
  }, [refreshData, props.token]);


  const goBackToListView = React.useCallback((refresh: boolean) => {
    setSelectedReport(null);
    setViewMode(View.List);
    if (refresh) {
      refreshData();
    }
  }, [refreshData]);
  const deleteInitiativeClick = React.useCallback((id: string) => {
    deleteSkillsInitiative(props.token, id).then(() => {
      refreshData();
      setViewMode(View.List);
    });
  }, [props.token, refreshData]);


  return (
    <div>
      {viewMode === View.Details ?
        <>
          {selectedReport &&
            <>
              <InitiativesDetails token={props.token} client={props.client}
                skillsInitiativeReport={selectedReport} />
              <Grid container>
                <Grid>
                  <button onClick={() => goBackToListView(false)} className="btn light">Back</button>
                </Grid>
                <Grid>
                  <button onClick={() => deleteInitiativeClick(selectedReport?.skillsInitiative.id)} className="btn">Delete</button>
                </Grid>
                <Grid>
                  <button onClick={() => setViewMode(View.AddEdit)} className="btn">Edit</button>
                </Grid>
              </Grid>
            </>
          }
        </>
        :
        <>
          {viewMode === View.List ?
            <>
              <h1>Your Skills Initiatives</h1>
              <p>These are the current, active initiatives your organisation has created to improve skills.</p>
              {skillsData ?
                <>
                  <section className="dashboard--summary smallpadbottom">
                    <div className="col-wrap key-stats">

                      <div className="col-03">
                        <div className="box-wrap">
                          <div className="stats-wrap">
                            <h3>Total Returns on Investments</h3>
                            <h2><MoneyLabel val={totalRoi} /></h2>
                          </div>
                        </div>
                      </div>

                      <div className="col-03">
                        <div className="box-wrap">
                          <div className="stats-wrap">
                            <h3>ROI vs Total Skills</h3>

                            <div style={{ display: 'flex' }}>
                              <div style={{ flex: 0, width: 100 }}>
                                <InitiativesRoiChart initiativesRoi={totalRoi} totalValue={skillsData.totalSkillsValue} />

                              </div>
                              
                              <div style={{ flex: 1, textAlign: 'left', marginLeft: 10 }}>
                                <p style={{ color: 'black' }}>Initiatives ROI</p>
                                <p style={{ color: ColourPicker.chartColours[0] }}>Skills total investment</p>
                              </div>
                            </div>

                          </div>
                        </div>
                      </div>
                    </div>
                  </section>

                  <table className='table'>
                    <thead>
                      <tr>
                        <th>Name</th>
                        <th>Trend</th>
                        <th>ROI</th>
                        <th colSpan={2}>% Complete</th>
                      </tr>
                    </thead>
                    <tbody>
                      {skillsData.reports.map(r => {
                        return <tr key={r.skillsInitiative.id}>
                          <td>{r.skillsInitiative.name}</td>
                          <td>
                            <TrendIcon percentIncrease={r.percentIncreaseFromStart} />
                          </td>
                          <td><MoneyLabel val={r.roi} /></td>
                          <td>
                            <PercentIcon percent={r.totalPercentComplete} />
                          </td>
                          <td>
                            <button onClick={() => selectReport(r)} style={{ margin: 0 }} className="btn">Select</button>
                          </td>
                        </tr>
                      })
                      }
                    </tbody>
                  </table>


                  <button onClick={() => setViewMode(View.AddEdit)} className="btn">New</button>

                </>
                :
                <LoadingSpinner />
              }
            </>
            :
            <>
              <InitiativesAddEdit token={props.token} client={props.client} existing={selectedReport?.skillsInitiative}
                cancelCallback={goBackToListView} initiativeSavedCallback={() => goBackToListView(true)} />
            </>
          }

        </>
      }

    </div >
  );
};
