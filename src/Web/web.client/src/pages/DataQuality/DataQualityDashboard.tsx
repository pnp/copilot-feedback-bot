
import React from 'react';
import { SkillsChartContainer } from '../../components/app/SkillsChartContainer';
import { DataQualityLineChart } from './DataQualityLineChart';
import 'chartjs-adapter-date-fns'
import { ChartView } from '../../apimodels/Enums';
import { DatasetControls } from '../../components/common/controls/DatasetControls';
import { DateOnlyLabel } from '../../components/common/controls/DateOnlyLabel';
import { NumberLabel } from '../../components/common/controls/NumberLabel';
import Tooltip, { TooltipProps, tooltipClasses } from '@mui/material/Tooltip';
import { CircleFigure } from '../../components/common/controls/CircleFigure';
import { styled } from '@mui/material/styles';

export const DataQualityDashboard: React.FC<{ token: string }> = () => {

    const [chartRange] = React.useState<ChartView>(ChartView.OneWeek);


    const BigTooltip = styled(({ className, ...props }: TooltipProps) => (
        <Tooltip {...props} classes={{ popper: className }} />
    ))(() => ({
        [`& .${tooltipClasses.tooltip}`]: {
            fontSize: 16,
        },
    }));


    return (
        <div>
            <section className="page--header">
                <div className="page-title">
                    <h1>Your Skills Data Quality</h1>
                    <p>Here's your key statistics as of <strong><DateOnlyLabel val={new Date()} /></strong> for <strong><NumberLabel val={16304} decimalPlaces={0} /> employees</strong>.</p>
                </div>
            </section>
            <DatasetControls newChartViewRange={() => null} loading={false} chartRange={ChartView.OneMonth} />
            <p>Pick a range to see progress this last period. </p>
            <h3>Progress</h3>
            <section className="dashboard--summary smallpadbottom">
                <div className="col-wrap key-stats">

                    <BigTooltip title="Precision (To review): How varied ratings are for any given employee/org. Big score variations shows low precision, i.e. if 'communication' get scores 1 through 6, then low precision.">
                        <div className="col-03">
                            <div className="box-wrap">
                                <div className="chart-wrap">
                                    <CircleFigure val={52} />
                                </div>
                                <div className="stats-wrap">
                                    <h3>Precision</h3>
                                    <p>+3%</p>
                                </div>
                                <div className="icon-wrap reports-up"></div>
                            </div>
                        </div>
                    </BigTooltip>

                    <BigTooltip title="Validity: What is the source of the skill signal, Inferred, self-rating, peer rating, manager rating, professional. Validity increases throughout the scale.">
                        <div className="col-03">
                            <div className="box-wrap">
                                <div className="chart-wrap">
                                    <CircleFigure val={16} />
                                </div>
                                <div className="stats-wrap">
                                    <h3>Validity</h3>
                                    <p>-11%</p>
                                </div>
                                <div className="icon-wrap reports-down"></div>

                            </div>
                        </div>
                    </BigTooltip>

                    <BigTooltip title="Completeness: Complete data contains all the necessary information required for analysis or decision-making. Check for missing skill values or incomplete records that could skew your analysis. How many skills do your uses have that don’t have ratings.​">
                        <div className="col-03">
                            <div className="box-wrap">
                                <div className="chart-wrap">
                                    <CircleFigure val={19} />
                                </div>
                                <div className="stats-wrap">
                                    <h3>Completeness</h3>
                                    <p>+3%</p>
                                </div>
                                <div className="icon-wrap reports-up"></div>
                            </div>
                        </div>
                    </BigTooltip>

                    <BigTooltip title="Consistency: Consistent data maintains uniformity and coherence across different datasets or data sources. Inconsistencies can arise from duplicate entries, conflicting information, or discrepancies in formatting. We look at users with different skill signals and the consistency of the signals.">
                        <div className="col-03">
                            <div className="box-wrap">
                                <div className="chart-wrap">
                                    <CircleFigure val={34} />
                                </div>
                                <div className="stats-wrap">
                                    <h3>Consistency</h3>
                                    <p>-5%</p>
                                </div>
                                <div className="icon-wrap reports-down"></div>
                            </div>
                        </div>
                    </BigTooltip>

                    <BigTooltip title="Timeliness: Timely data is up-to-date and reflects the most recent information available. When was the last time your skill data was verified. Skills lose value over time, Timeliness manages how recent your skills were rated​">
                        <div className="col-03">
                            <div className="box-wrap">
                                <div className="chart-wrap">
                                    <CircleFigure val={26} />
                                </div>
                                <div className="stats-wrap">
                                    <h3>Timeliness</h3>
                                    <p>+12%</p>
                                </div>
                                <div className="icon-wrap reports-up"></div>
                            </div>
                        </div>
                    </BigTooltip>

                    <BigTooltip title="Accuracy: Good data should accurately reflect the real-world entities or events it represents. We asses this by looking at peers (job title) and Job Family data">
                        <div className="col-03">
                            <div className="box-wrap">
                                <div className="chart-wrap">
                                    <CircleFigure val={18} />
                                </div>
                                <div className="stats-wrap">
                                    <h3>Accuracy</h3>
                                    <p>-4%</p>
                                </div>
                                <div className="icon-wrap reports-down"></div>
                            </div>
                        </div>
                    </BigTooltip>

                    <BigTooltip title="Overrall score: a sum of all the above stats in a single number">
                        <div className="col-01">
                            <div className="box-wrap">
                                <div className="icon-wrap quality"></div>

                                <div style={{ display: "flex" }}>
                                    <div style={{ flexGrow: 1 }}>
                                        <h2>25%</h2>
                                        <p><strong>Overrall Quality</strong></p>
                                    </div>
                                </div>

                                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                                    <h2 className="large-number">Groups</h2>
                                    <p>All</p>
                                </div>
                                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                                    <h2 className="large-number">Cohorts</h2>
                                    <p>All</p>
                                </div>
                                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                                    <h2 className="number">Initiatives</h2>
                                    <p>All</p>
                                </div>
                                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                                    <h2 className="number">Business Units</h2>
                                    <p>All</p>
                                </div>
                                <div style={{ flexGrow: 1, paddingLeft: 50 }}>
                                    <button className="btn">Filter</button>
                                </div>
                            </div>

                        </div>
                    </BigTooltip>
                </div>
            </section>

            <h1>Skills Quality Evolution</h1>
            <p>Here's how the quality of your skills data has changed over time.</p>

            <SkillsChartContainer>
                <div style={{ marginTop: 20 }}>

                    <DataQualityLineChart chartRange={chartRange} loading={false} />

                </div>

            </SkillsChartContainer>
            <h1 style={{ marginTop: 10 }}>Skills Quality Highs & Lows</h1>
            <p></p>
            <section className="dashboard--summary smallpadbottom">
                <div className="col-wrap key-stats">
                    <div className="col-03">
                        <div className="box-wrap">
                            <div className="stats-wrap">
                                <h3>High (All Time)</h3>
                                <h2 style={{ fontSize: 20 }}>8.1/10</h2>
                                <p><strong><DateOnlyLabel val={new Date()} /></strong></p>
                            </div>
                        </div>
                    </div>
                    <div className="col-03">
                        <div className="box-wrap">
                            <div className="stats-wrap">
                                <h3>Low (All Time)</h3>
                                <h2 style={{ fontSize: 20 }}>4.6/10</h2>
                                <p><strong>14-01-2023</strong></p>
                            </div>
                        </div>
                    </div>
                    <div className="col-03">
                        <div className="box-wrap">
                            <div className="stats-wrap">
                                <h3>Biggest Improvement</h3>
                                <h2 style={{ fontSize: 20 }}>12%</h2>
                                <p><strong>January - Feburary</strong></p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

        </div>
    );
};
