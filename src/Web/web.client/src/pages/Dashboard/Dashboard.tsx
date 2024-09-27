
import React from 'react';
import { loadDashboardChartAndStats, loadSkillsTree, loadUserProfile } from '../../api/ApiCalls';
import { SkillsChartContainer } from '../../components/app/SkillsChartContainer';
import { TrendsOverviewChart } from './TrendsOverviewChart';
import 'chartjs-adapter-date-fns'
import { TreeData, convertSkillsToTreeNode } from '../../components/common/controls/SkillsTree';
import { ChartView } from '../../apimodels/Enums';
import { DatasetControls } from '../../components/common/controls/DatasetControls';
import { DashboardSectionSkillKPIs } from './DashboardSectionSkillKPIs';
import { Filters } from './Filters';
import { getDateFromNow } from '../../utils/DataUtils';
import { LoadingSpinner } from '../../components/common/controls/LoadingSpinner';
import { MoneyLabel } from '../../components/common/controls/MoneyLabel';
import { DateOnlyLabel } from '../../components/common/controls/DateOnlyLabel';

export const Dashboard: React.FC<{ token: string, profileLoaded: Function }> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [skillsTreeData, setSkillsTreeData] = React.useState<TreeData[] | null>(null);
  const [selectedSkillFilter, setselectedSkillFilter] = React.useState<TreeData | null>(null);

  const [loginProfile, setLoginProfile] = React.useState<LoginProfile | null>(null);
  const [loadingProfile, setLoadingProfile] = React.useState<boolean>(true);

  const [dashboardChartAndStats, setDashboardChartAndStats] = React.useState<DashboardChartAndStats | null>();

  const [chartRange, setChartRange] = React.useState<ChartView>(ChartView.OneWeek);

  // Send up to app
  const setProfileContext = React.useCallback((profile: LoginProfile | null) => {
    setLoginProfile(profile);
    props.profileLoaded(profile);
  }, [props]);


  React.useEffect(() => {
    loadSkillsTree(props.token)
      .then(async response => {
        setSkillsTreeData(convertSkillsToTreeNode(response));
      });

    // Load user profile
    if (!loginProfile)
      loadUserProfile(props.token)
        .then(async response => {
          setLoadingProfile(false);

          setActiveLoginProfileAndRefreshStats(response);

        })
        .catch(() => {
          setLoadingProfile(false);
        });

    // eslint-disable-next-line
  }, [selectedSkillFilter]);

  const setActiveLoginProfileAndRefreshStats = (profile: LoginProfile | null) => {
    setProfileContext(profile);
  }

  const loadDashboard = React.useCallback((length: ChartView) => {

    const dateFrom = getDateFromNow(length);

    setLoading(true);

    loadDashboardChartAndStats({ from: dateFrom, to: new Date(), skillsIdFilter: selectedSkillFilter?.id }, props.token)
      .then(async response => {
        setDashboardChartAndStats(response);
        setLoading(false);
      });
  }, [props.token, selectedSkillFilter?.id]);


  const [openFilterModal, setOpenFilterModal] = React.useState<boolean>(false);
  const handleOpen = () => setOpenFilterModal(true);
  const handleClose = () => setOpenFilterModal(false);
  
  React.useEffect(() => {
    loadDashboard(chartRange);
  }, [chartRange, selectedSkillFilter, loadDashboard]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Skills Overview</h1>
          <p>Here's your organisation skills info.</p>
        </div>
      </section>

      {loadingProfile ?
        <p><LoadingSpinner /></p>
        :
        <>
          <section className="dashboard--summary smallpadbottom">
            <div className="col-wrap key-stats">
              <DashboardSectionSkillKPIs companyOverviewStats={dashboardChartAndStats?.companyOverviewStats} 
                pcChange={dashboardChartAndStats?.skillsDataSummary.metadata.filteredStats.percentDiffInView} />
            </div>
          </section>

          <SkillsChartContainer>
            {loginProfile &&
              <h1>Skills Value for {loginProfile.client.name}</h1>
            }
            <DatasetControls newChartViewRange={(r: ChartView) => setChartRange(r)} chartRange={chartRange} loading={loading} filterOptionClicked={handleOpen} />
            {dashboardChartAndStats &&
              <>
                <TrendsOverviewChart skillsIdFilter={selectedSkillFilter?.id} chartRange={chartRange} skillsData={dashboardChartAndStats.skillsDataSummary} loading={loading} />
              </>
            }

          </SkillsChartContainer>

          {skillsTreeData &&
            <Filters selectedSkillFilter={selectedSkillFilter} skillsTreeData={skillsTreeData}
              setselectedSkillFilter={(t: TreeData) => setselectedSkillFilter(t)}
              handleClose={handleClose} handleOpen={handleOpen} openFilterModal={openFilterModal} />
          }

          <section className="dashboard--summary smallpadbottom">
            <div className="col-wrap key-stats">
              <div className="col-03">
                <div className="box-wrap">
                  <div className="stats-wrap">
                    <h3>High (in View)</h3>
                    <h2 style={{ fontSize: 20 }}><MoneyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.filteredStats.highestValue?.value} /></h2>
                    <p><strong><DateOnlyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.filteredStats.highestValue?.when} /></strong></p>
                  </div>
                </div>
              </div>

              <div className="col-03">
                <div className="box-wrap">
                  <div className="stats-wrap">
                    <h3>Low (in View)</h3>
                    <h2 style={{ fontSize: 20 }}><MoneyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.filteredStats.lowestValue?.value} /></h2>
                    <p><strong><DateOnlyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.filteredStats.lowestValue?.when} /></strong></p>
                  </div>
                </div>
              </div>
              
              <div className="col-03">
                <div className="box-wrap">
                  <div className="stats-wrap">
                    <h3>Last Updated</h3>
                    <h2 style={{ fontSize: 20 }}><strong><DateOnlyLabel val={dashboardChartAndStats?.companyOverviewStats?.lastRecordedSkillData} /></strong></h2>
                  </div>
                </div>
              </div>

            </div>
          </section>

          <section className="dashboard--summary smallpadbottom">
            <div className="col-wrap key-stats">
              <div className="col-03">
                <div className="box-wrap">
                  <div className="stats-wrap">
                    <h3>High (All Time)</h3>
                    <h2 style={{ fontSize: 20 }}><MoneyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.allTimeStats.highestValue?.value} /></h2>
                    <p><strong><DateOnlyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.allTimeStats.highestValue?.when} /></strong></p>
                  </div>
                </div>
              </div>

              <div className="col-03">
                <div className="box-wrap">
                  <div className="stats-wrap">
                    <h3>Low (All Time)</h3>
                    <h2 style={{ fontSize: 20 }}><MoneyLabel val={dashboardChartAndStats?.skillsDataSummary?.metadata.allTimeStats.lowestValue?.value} /></h2>
                    <p><strong><DateOnlyLabel val={dashboardChartAndStats?.skillsDataSummary.metadata.allTimeStats.lowestValue?.when} /></strong></p>
                  </div>
                </div>
              </div>

            </div>
          </section>
        </>
      }
    </div >
  );
};
