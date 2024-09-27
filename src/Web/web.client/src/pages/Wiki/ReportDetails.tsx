
import React from 'react';

export const ReportDetails: React.FC<{ report: JobTitleReport }> = (props) => {

    return (
        <div>

            <h3>Job Titles</h3>
            <table>
                <thead>
                    <tr>
                        <th>Job Title</th>
                        <th>Skills</th>
                    </tr>
                </thead>
                <tbody>
                    {props.report.jobTitlesAndSkills.map(jts => (
                        <tr key={jts.jobTitleName}>
                            <td>{jts.jobTitleName}</td>
                            <td>{jts.skills.join(', ')}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <h3>Skills</h3>
            <table>
                <thead>
                    <tr>
                        <th>Skill</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    {props.report.skillNameAndDescriptions.map(snd => (
                        <tr key={snd.name}>
                            <td>{snd.name}</td>
                            <td>{snd.description}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};
