
import React from 'react';
import { findSkillsLightCast } from '../../api/ApiCalls';
import { MenuItem, Select, SelectChangeEvent } from '@mui/material';

export const JobTitleSuggestions: React.FC<{ token: string, suggestion: string, callback: Function }> = (props) => {

    const [searchJobTitlesResults, setSearchJobTitlesResults] = React.useState<NameAndId[] | null>(null);
    const [selectedId, setSelectedId] = React.useState<string | null>(null);

    const lookupTitles = React.useCallback(() => {

        // Load from API
        findSkillsLightCast(props.token, props.suggestion).then(response => {
            setSearchJobTitlesResults(response);
        });
    }, [props.token]);

    React.useEffect(() => {
        lookupTitles();
    }, [lookupTitles]);

    const handleChange = (event: SelectChangeEvent) => {

        const selectedTitleId = event.target.value as string;
        setSelectedId(selectedTitleId);

        // Find the selected job title
        const selectedJobTitle = searchJobTitlesResults?.find(jt => jt.id === selectedTitleId);
        if (selectedJobTitle) {
            props.callback(selectedJobTitle);
        }
    };
    return (
        <>
            <p>
                Search for '{props.suggestion}'
                {searchJobTitlesResults && searchJobTitlesResults.length === 0 && ' - No results found'}
                {searchJobTitlesResults && searchJobTitlesResults.length > 0 &&
                    <>- {searchJobTitlesResults.length} results found</>
                }
            </p>
            {searchJobTitlesResults &&

                <Select
                    value={selectedId ?? ''}
                    label="Job Title"
                    onChange={handleChange}>
                    {searchJobTitlesResults.map((jt, idx) => (

                        <MenuItem value={jt.id} selected={idx === 0}>{jt.name}</MenuItem>

                    ))}
                </Select>
            }
        </>
    );
};
