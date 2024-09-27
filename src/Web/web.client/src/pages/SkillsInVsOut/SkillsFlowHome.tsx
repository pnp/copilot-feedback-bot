
import React from 'react';
import { loadSkillsInsightsReports } from '../../api/ApiCalls';
import { SkillsByJobTitleChart } from '../../components/common/charts/SkillsByJobTitleChart';
import { LoadingSpinner } from '../../components/common/controls/LoadingSpinner';
import { SkillsTrendsChart } from '../../components/common/charts/SkillsTrendsChart';
import { SkillsChartContainer } from '../../components/app/SkillsChartContainer';
import { ChartView } from '../../apimodels/Enums';
import { DatasetControls } from '../../components/common/controls/DatasetControls';
import { getDateFromNow } from '../../utils/DataUtils';
import { SkillsInVsOutSankey } from './SkillsInVsOutSankey';

export const SkillsFlowHome: React.FC<{ token: string, client: Client }> = (props) => {

  const [chartRange, setChartRange] = React.useState<ChartView>(ChartView.OneWeek);

  const [skillsData, setSkillsData] = React.useState<SkillsInsightsReports>();

  const refreshGraphs = React.useCallback(() => {

    // Load company sentiment stats
    const selectedFrom = getDateFromNow(chartRange);
    const newFilter: ToFromModelLoadFilter = { from: selectedFrom, to: new Date() };

    // Demograph stats
    loadSkillsInsightsReports(newFilter, props.token)
      .then(async response => {
        setSkillsData(response);
      });

    window.initMainJs();
  }, [props.token, chartRange]);

  React.useEffect(() => {
    refreshGraphs();
  }, [refreshGraphs]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Skills in vs Out</h1>
          <p>See who's leaving, from where.</p>
        </div>
      </section>

      <section className="reports--head">
        <div className="date-selector">
          <DatasetControls newChartViewRange={(r: ChartView) => setChartRange(r)} chartRange={chartRange} loading={false} />
        </div>

      </section>

      <SkillsChartContainer>

        <section className="reports--chart nopad">

          <ul id="tabs">
            <li className="overview active">Overview</li>
            <li className="demographics">Details</li>
          </ul>

        </section>
        <>
          <ul id="tab">
            <li className="overview active">
              <div className="box-wrap graph-wrap">
                <h2>Skills Lost - All</h2>
                <p>This shows the flow of total skills value within the organization for the selected time period. Skill value retained and-or lost</p>
                {skillsData &&
                  <SkillsInVsOutSankey chartRange={chartRange} skillsData={skillsData.skillsTrendAll} loading={false} />
                }
              </div>
            </li>

            <li className="demographics">
              <div className="box-wrap graph-wrap">
                <h2>Skills Lost by Job Title</h2>
                <p>All employee skills data, averaged by job title, by date skills were registered.</p>
                {skillsData?.skillsByJobTitle ?
                  <SkillsByJobTitleChart data={skillsData?.skillsByJobTitle} showActive={false} />
                  :
                  <LoadingSpinner />
                }
                <br />
                <h2>Skill Lost Trends by Skill Area</h2>
                {skillsData?.skillsBySkillName ?
                  <SkillsTrendsChart data={skillsData.skillsBySkillName} showActive={false} />
                  :
                  <LoadingSpinner />
                }
              </div>
            </li>
          </ul>
        </>
      </SkillsChartContainer>
    </div >
  );
};

