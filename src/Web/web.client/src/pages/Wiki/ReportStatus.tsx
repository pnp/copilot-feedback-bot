
import React from 'react';
import { getJobTitlesReport } from '../../api/ApiCalls';
import { ReportDetails } from './ReportDetails';

export const ReportStatus: React.FC<{ token: string, report: JobTitleReportResponse }> = (props) => {

    const [jobTitleReportResponse, setJobTitleReportResponse] = React.useState<JobTitleReportResponse>(props.report);

    // Refresh sesh
    React.useEffect(() => {

        if (jobTitleReportResponse.completed !== null) {
            return;
        }
        console.log(`Initialising refresh interval`);
        const interval = setInterval(() => {
            getJobTitlesReport(props.token, props.report.reportId).then((s) => setJobTitleReportResponse(s));
        }, 5000);

        return () => {
            console.log(`Clearing interval`);
            clearInterval(interval);
        };
    }, [jobTitleReportResponse]);


    return (
        <div>

            {jobTitleReportResponse.completed === null ?
                <h3>Report is being processed...</h3>
                :
                <>
                    {jobTitleReportResponse.report ?
                        <ReportDetails report={jobTitleReportResponse.report} />
                        :
                        <h3>Report Complete?</h3>
                    }
                </>
            }

            <pre>
                {JSON.stringify(jobTitleReportResponse, null, 2)}
            </pre>
        </div>
    );
};
