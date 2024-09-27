import React from 'react';
import { MoneyLabel } from '../../components/common/controls/MoneyLabel';
import { NumberLabel } from '../../components/common/controls/NumberLabel';

export const DashboardSectionSkillKPIs: React.FC<{ companyOverviewStats?: CompanyOverviewStats, pcChange?: number }> = (props) => {

  // Init animations once we have global stats
  React.useEffect(() => {


    if (props.companyOverviewStats)
      window.initCharts(props.companyOverviewStats.totalValueStats.thisQuarterValuePercentageChangeFromPrevious,
        props.companyOverviewStats.confidenceStats.thisMonthValuePercentageChangeFromPrevious,
        props.companyOverviewStats.confidenceStats.thisWeekValuePercentageChangeFromPrevious);

  }, [props.companyOverviewStats]);


  const perCapita = props.companyOverviewStats ?
    (props.companyOverviewStats.currentValueTotal / props.companyOverviewStats.employeeCount) : undefined;

  return (
    <>
      <div className="col-03">
        <div className="box-wrap">
          <div className="stats-wrap">
            <h3>Total Skills Value</h3>
            <h2><MoneyLabel val={props.companyOverviewStats?.currentValueTotal} /></h2>
            <p><strong><NumberLabel val={props.companyOverviewStats?.currentConfidence} decimalPlaces={2} />% confidence</strong></p>
          </div>
        </div>
      </div>

      <div className="col-03">
        <div className="box-wrap icon">
          <div className="icon-wrap employees"></div>

          <div className="stats-wrap">
            {props.companyOverviewStats &&
              <>
                <h2><MoneyLabel val={perCapita} /></h2>
                <p><strong>Current per-capita value for {props.companyOverviewStats.employeeCount.toLocaleString()} employees</strong></p>
              </>
            }
          </div>
        </div>
      </div>

      <div className="col-03">
        <div className="box-wrap icon">

          {!props.pcChange || props.pcChange > 1 ?
            <div className="icon-wrap reports-up"></div>
            :
            <div className="icon-wrap reports-down"></div>
          }

          <div className="stats-wrap">
            <h2><NumberLabel val={props.pcChange} decimalPlaces={2} />%</h2>
            <p><strong>Value Change</strong></p>
          </div>
        </div>
      </div>
    </>
  );
};
