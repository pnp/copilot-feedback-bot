
import React, { ChangeEvent } from 'react';
import { TextField } from '@mui/material';
import { JobTitleSuggestions } from './JobTitleSuggestions';
import { startProcessingJobTitlesReport } from '../../api/ApiCalls';
import { ReportStatus } from './ReportStatus';

interface JobTitleForSuggestion {
    suggestion: string;
    jobTitle: NameAndId;

}

export const WikiHome: React.FC<{ token: string }> = (props) => {

    const [jobTitlesInput, setJobTitlesInput] = React.useState<string>(".net, c#");
    const [jobTitlesWanted, setJobTitlesWanted] = React.useState<string[] | null>(null);
    const [jobTitlesSelected, setJobTitlesSelected] = React.useState<JobTitleForSuggestion[]>([]);
    const [jobTitleReportResponse, setJobTitleReportResponse] = React.useState<JobTitleReportResponse | null>(null);

    const lookupTitles = React.useCallback(() => {
        if (jobTitlesInput === '') {
            alert('Enter job title query text');
            return;
        }

        // Split the job titles
        setJobTitlesWanted(jobTitlesInput.split(',').map(jt => jt.trim()));

    }, [jobTitlesWanted, props.token]);


    const startProcessing = React.useCallback(() => {
        startProcessingJobTitlesReport(props.token, jobTitlesSelected.map(jts => jts.jobTitle.id))
            .then(response => setJobTitleReportResponse(response));

    }, [jobTitlesSelected, props.token]);

    const jobTitleSelected = React.useCallback((forSuggestion: string, jobTitleSelected: NameAndId) => {

        // Find the existing suggestion
        const existingSuggestion = jobTitlesSelected.find(jt => jt.suggestion === forSuggestion);
        if (existingSuggestion) {
            existingSuggestion.jobTitle = jobTitleSelected;
        } else {
            setJobTitlesSelected([...jobTitlesSelected, { suggestion: forSuggestion, jobTitle: jobTitleSelected }]);
        }

    }, [jobTitlesSelected]);

    return (
        <div>
            <section className="page--header">
                <div className="page-title">
                    <h1>Skills Research</h1>
                    <p>Look things up.</p>
                </div>
            </section>

            {jobTitleReportResponse ?
                <ReportStatus report={jobTitleReportResponse} token={props.token} />
                :
                <>
                    {jobTitlesWanted ?
                        <>
                            <h3>2/3 Confirm Job Titles</h3>
                            <table>
                                <tbody>
                                    {jobTitlesWanted.map((jt, idx) => (
                                        <tr key={idx}>
                                            <td>
                                                <JobTitleSuggestions token={props.token} suggestion={jt} callback={(s: NameAndId) => jobTitleSelected(jt, s)} />
                                            </td>
                                            <td>
                                                {jobTitlesSelected.find(jts => jts.suggestion === jt)?.jobTitle?.name}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </>
                        :
                        <>
                            <h3>1/3 - Find a Job Titles</h3>
                            <TextField label="Job Titles, comma-seperated" required value={jobTitlesInput} onChange={(e: ChangeEvent<HTMLInputElement>) => setJobTitlesInput(e.target.value)} />
                            <button onClick={lookupTitles} className="btn dark">Search</button>
                        </>
                    }

                    <p>Selected Job Titles: {jobTitlesSelected.length}</p>
                    {jobTitlesSelected.length > 0 &&
                        <button onClick={startProcessing} className="btn dark">Build Report</button>
                    }
                </>
            }


        </div>
    );
};
