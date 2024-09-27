import React from 'react';
import { loadCompanyOverviewStats, loadSkillsInsightsReports } from '../../api/ApiCalls';
import { CompanyComparisonChart } from './CompanyComparisonChart';
import './ClientArea.css';
import { SkillsByJobTitleChart } from '../../components/common/charts/SkillsByJobTitleChart';
import { SkillsPopularityChart } from './SkillsPopularityChart';
import { LoadingSpinner } from '../../components/common/controls/LoadingSpinner';
import { SkillsTrendsChart } from '../../components/common/charts/SkillsTrendsChart';
import { SkillsChartContainer } from '../../components/app/SkillsChartContainer';
import { DashboardSectionSkillTrendCharts } from './DashboardSectionSkillTrendCharts';
import { TrendLabel } from '../../components/common/controls/TrendLabel';
import { DatasetControls } from '../../components/common/controls/DatasetControls';
import { ChartView, DataViewSetting } from '../../apimodels/Enums';
import { getDateFromNow } from '../../utils/DataUtils';
import { NumberLabel } from '../../components/common/controls/NumberLabel';
import { DataViewSettingControl } from '../../components/common/controls/DataViewSettingControl';

export const SkillsInsights: React.FC<{ token: string, client: Client }> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [skillsData, setSkillsData] = React.useState<SkillsInsightsReports>();
  const [chartRange, setChartRange] = React.useState<ChartView>(ChartView.OneWeek);

  const [companyOverviewStats, setCompanyOverviewStats] = React.useState<CompanyOverviewStats | null>();

  const refreshGraphs = React.useCallback(() => {

    // Load company sentiment stats
    setLoading(true);
    const dateFrom = getDateFromNow(chartRange);
    const newFilter: ToFromModelLoadFilter = { from: dateFrom, to: new Date() };

    // Demograph stats
    loadSkillsInsightsReports(newFilter, props.token)
      .then(async response => {
        setSkillsData(response);
        setLoading(false);
      });

    window.initMainJs();
  }, [props.token, chartRange]);


  React.useEffect(() => {
    refreshGraphs();

    // Load client stats
    if (!companyOverviewStats)
      loadCompanyOverviewStats(props.token)
        .then(async response => {

          if (response)
            window.initCharts(response.totalValueStats.thisQuarterValuePercentageChangeFromPrevious,
              response.totalValueStats.thisMonthValuePercentageChangeFromPrevious,
              response.totalValueStats.thisWeekValuePercentageChangeFromPrevious);
          setCompanyOverviewStats(response);
        });

  }, [refreshGraphs, companyOverviewStats, props.token]);


  return (
    <div>
      <h1>Your Skills Insights</h1>
      <p>Skills progression and key stats are shown here.</p>

      <SkillsChartContainer>

        <section className="dashboard--summary smallpadbottom">
          <div className="col-wrap key-stats">


            <div className="col-01">
              <div className="box-wrap">
                <div style={{ display: "flex" }}>
                  <div style={{ flexGrow: 1 }}>
                    <h2>${companyOverviewStats?.currentValueTotal.toLocaleString()}</h2>
                    <p><strong><NumberLabel val={companyOverviewStats?.currentConfidence} decimalPlaces={2} />% confidence</strong></p>
                  </div>
                </div>

                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                  <h2 className="large-number">{companyOverviewStats?.employeeCount}</h2>
                  <p>Employees</p>
                </div>
                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                  <h2 className="large-number">{companyOverviewStats?.dataPoints}</h2>
                  <p>Data Points</p>
                </div>
                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                  <h2 className="number">{companyOverviewStats?.uniqueSkillsCount}</h2>
                  <p>Skills</p>
                </div>
              </div>

            </div>
          </div>
        </section>

        <h4>View Skills Statistics As</h4>
        <DataViewSettingControl setting={DataViewSetting.Financial} loading={false} />

        <section className="dashboard--summary smallpadbottom">
          <div className="col-wrap key-stats">

            <div className="col-03">
              <div className="box-wrap">
                <div className="chart-wrap">
                  <div className="circle" id="overall-circle"></div>
                </div>

                <div className="stats-wrap">
                  <h3>3 Months ago</h3>
                  <h2>${companyOverviewStats?.totalValueStats.lastQuarterValueTotal.toLocaleString()}</h2>

                  <TrendLabel label='Value' val={companyOverviewStats?.totalValueStats.thisQuarterValuePercentageChangeFromPrevious ?? 0} />
                  <TrendLabel label='Conf' classNameOverride='' val={companyOverviewStats?.confidenceStats.thisQuarterValuePercentageChangeFromPrevious ?? 0} />

                </div>
              </div>
            </div>

            <div className="col-03">
              <div className="box-wrap">
                <div className="chart-wrap">
                  <div className="circle" id="30day-circle"></div>
                </div>

                <div className="stats-wrap">
                  <h3>30-Days ago</h3>
                  <h2>${companyOverviewStats?.totalValueStats.lastMonthValueTotal.toLocaleString()}</h2>
                  <TrendLabel label='Value' val={companyOverviewStats?.totalValueStats.thisMonthValuePercentageChangeFromPrevious ?? 0} />
                  <TrendLabel label='Conf' classNameOverride='' val={companyOverviewStats?.confidenceStats.thisMonthValuePercentageChangeFromPrevious ?? 0} />
                </div>
              </div>
            </div>

            <div className="col-03">
              <div className="box-wrap">
                <div className="chart-wrap">
                  <div className="circle" id="7day-circle"></div>
                </div>

                <div className="stats-wrap">
                  <h3>7-Days ago</h3>
                  <h2>${companyOverviewStats?.totalValueStats.lastWeekValueTotal.toLocaleString()}</h2>
                  <TrendLabel label='Value' val={companyOverviewStats?.totalValueStats.thisWeekValuePercentageChangeFromPrevious ?? 0} />
                  <TrendLabel label='Conf' classNameOverride='' val={companyOverviewStats?.confidenceStats.thisWeekValuePercentageChangeFromPrevious ?? 0} />
                </div>
              </div>
            </div>

          </div>
        </section>

        <h4>Deep-Dive</h4>
        <DatasetControls newChartViewRange={(r: ChartView) => setChartRange(r)} chartRange={chartRange} loading={loading} />
        <div style={{ paddingTop: 50 }}></div>
        <section className="reports--chart nopad">

          <ul id="tabs">
            <li className="demographics active">Trends</li>
            <li className="byJobTitle">By Job Title</li>
            <li className="channels">Skill Areas</li>
            <li className="comparison">Industry Comparison</li>
          </ul>

        </section>
        <>
          <ul id="tab">

            <li className="demographics active">

              <div className="box-wrap graph-wrap">
                <h2>Skills Trends</h2>
                <p>An overview of skills data.</p>
                {skillsData?.skillsDeviation && skillsData?.skillsTrendAll ?
                  <DashboardSectionSkillTrendCharts skillsDeviation={skillsData.skillsDeviation}
                    datesCovered={skillsData.skillsTrendAll.metadata.datesCovered} chartRange={chartRange} />
                  :
                  <LoadingSpinner />
                }
              </div>
            </li>

            <li className="byJobTitle">
              <div className="box-wrap graph-wrap">
                <h2>Skill by Job Title</h2>
                <p>All employee skills data, averaged by job title, by date skills were registered.</p>
                {skillsData?.skillsByJobTitle ?
                  <SkillsByJobTitleChart data={skillsData.skillsByJobTitle} showActive={true} />
                  :
                  <LoadingSpinner />
                }
              </div>
            </li>

            <li className="channels">
              <div className="box-wrap graph-wrap">
                <h2>Skill Trends by Skill Area</h2>
                <p>All skills registered by all employees, by quantity of data.</p>
                {skillsData?.skillsPopularity &&
                  <SkillsPopularityChart data={skillsData.skillsPopularity} />
                }

                <p>Value by skill name</p>
                {skillsData?.skillsBySkillName ?
                  <SkillsTrendsChart data={skillsData.skillsBySkillName} showActive={true} />
                  :
                  <LoadingSpinner />
                }
              </div>
            </li>

            {skillsData?.skillsStatsCompanyComparison &&
              <li className="comparison">
                <div className="box-wrap graph-wrap">
                  <h2>Industry Comparison</h2>
                  <p>Here's how your skills levels compares to others being tracked.</p>
                  <CompanyComparisonChart data={skillsData.skillsStatsCompanyComparison} />
                </div>
              </li>
            }

          </ul>
        </>

      </SkillsChartContainer>
    </div >
  );
};

