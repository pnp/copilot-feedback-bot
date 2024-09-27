import React, { ChangeEvent } from "react";
import { Box, Checkbox, FormControlLabel, FormGroup, TextField } from "@mui/material";

export const ImportConfigurationsAddEdit: React.FC<{ model?: ImportConfiguration, saveCallback: Function, cancelCallback: Function }> = (props) => {

    const [name, setName] = React.useState<string>("");

    const [ratingColumnIdx, setRatingColumnIdx] = React.useState<number>(0);
    const [ratingTypeColumnIdx, setRatingTypeColumnIdx] = React.useState<number>(0);
    const [ratingSourceColumnIdx, setRatingSourceColumnIdx] = React.useState<number>(0);
    const [ratingDateColumnIdx, setRatingDateColumnIdx] = React.useState<number>(0);
    const [employeeIdColumnIdx, setEmployeeIdColumnIdx] = React.useState<number>(0);
    const [jobTitleColumnIdx, setJobTitleColumnIdx] = React.useState<number>(0);
    const [jobFamilyColumnIdx, setJobFamilyColumnIdx] = React.useState<number>(0);
    const [jobFamilyGroupColumnIdx, setJobFamilyGroupColumnIdx] = React.useState<number>(0);
    const [skillNameColumnIdx, setSkillNameColumnIdx] = React.useState<number>(0);
    const [skillPlanColumnIdx, setSkillPlanColumnIdx] = React.useState<number>(0);
    const [maxSkillRating, setMaxSkillRating] = React.useState<number>(0);
    const [firstLineIsHeader, setFirstLineIsHeader] = React.useState<boolean>(true);

    const getNumber = React.useCallback((e: ChangeEvent<HTMLInputElement>): number => {
        const value = !Number.isNaN(e.target.valueAsNumber) ? e.target.valueAsNumber : null;
        return (value ?? 0);
    }, []);

    React.useEffect(() => {
        if (props.model) {
            setName(props.model.name);
            setRatingColumnIdx(props.model.ratingColumnIdx);
            setRatingTypeColumnIdx(props.model.ratingTypeColumnIdx);
            setRatingSourceColumnIdx(props.model.ratingSourceColumnIdx);
            setRatingDateColumnIdx(props.model.ratingDateColumnIdx);
            setEmployeeIdColumnIdx(props.model.employeeIdColumnIdx);
            setJobTitleColumnIdx(props.model.jobTitleColumnIdx);
            setJobFamilyColumnIdx(props.model.jobFamilyColumnIdx);
            setJobFamilyGroupColumnIdx(props.model.jobFamilyGroupColumnIdx);
            setSkillNameColumnIdx(props.model.skillNameColumnIdx);
            setSkillPlanColumnIdx(props.model.skillPlanColumnIdx);
            setMaxSkillRating(props.model.maxSkillRating);
            setFirstLineIsHeader(props.model.firstLineIsHeader);
        }
    }, [props.model]);

    const save = React.useCallback(() => {
        if (name === '') {
            alert('Enter name');
            return;
        }

        const r: ImportConfiguration =
        {
            id: props.model?.id ?? "",
            name: name,
            employeeIdColumnIdx: employeeIdColumnIdx,
            firstLineIsHeader: firstLineIsHeader,
            jobFamilyColumnIdx: jobFamilyColumnIdx,
            jobFamilyGroupColumnIdx: jobFamilyGroupColumnIdx,
            jobTitleColumnIdx: jobTitleColumnIdx,
            maxSkillRating: maxSkillRating,
            ratingColumnIdx: ratingColumnIdx,
            ratingDateColumnIdx: ratingDateColumnIdx,
            ratingSourceColumnIdx: ratingSourceColumnIdx,
            ratingTypeColumnIdx: ratingTypeColumnIdx,
            skillNameColumnIdx: skillNameColumnIdx,
            skillPlanColumnIdx: skillPlanColumnIdx
        }
        props.saveCallback(r);
    }, [employeeIdColumnIdx, firstLineIsHeader, jobFamilyColumnIdx, jobFamilyGroupColumnIdx, jobTitleColumnIdx, maxSkillRating, ratingColumnIdx, 
        ratingDateColumnIdx, ratingSourceColumnIdx, ratingTypeColumnIdx, skillNameColumnIdx, skillPlanColumnIdx, name, props]);

    const cancel = React.useCallback(() => {

        props.cancelCallback();
    }, [props]);

    return (
        <>
            {props.model ?
                <h3>Edit Import Format - '{props.model.name}'</h3>
                :
                <h3>New Import Format</h3>
            }

            <Box
                component="form"
                autoComplete="off"
            >

                <TextField label="Name" required value={name} onChange={(e: ChangeEvent<HTMLInputElement>) => setName(e.target.value)} />
                <TextField label="Rating Column"
                    type="number" value={ratingColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setRatingColumnIdx(getNumber(e))} />
                <TextField label="Rating Type Column"
                    type="number" value={ratingTypeColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setRatingTypeColumnIdx(getNumber(e))} />
                <TextField label="Rating Source Column"
                    type="number" value={ratingSourceColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setRatingSourceColumnIdx(getNumber(e))} />
                <TextField label="Rating Date Column"
                    type="number" value={ratingDateColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setRatingDateColumnIdx(getNumber(e))} />
                <TextField label="Employee Id Column"
                    type="number" value={employeeIdColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setEmployeeIdColumnIdx(getNumber(e))} />
                <TextField label="Job Title Column"
                    type="number" value={jobTitleColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setJobTitleColumnIdx(getNumber(e))} />
                <TextField label="Job Family ColumnIdx"
                    type="number" value={jobFamilyColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setJobFamilyColumnIdx(getNumber(e))} />
                <TextField label="Job Family Group Column"
                    type="number" value={jobFamilyGroupColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setJobFamilyGroupColumnIdx(getNumber(e))} />
                <TextField label="Skill Name Column"
                    type="number" value={skillNameColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setSkillNameColumnIdx(getNumber(e))} />
                <TextField label="Skill Plan Column"
                    type="number" value={skillPlanColumnIdx} onChange={(e: ChangeEvent<HTMLInputElement>) => setSkillPlanColumnIdx(getNumber(e))} />
                <TextField label="Max Skill Rating"
                    type="number" value={maxSkillRating} onChange={(e: ChangeEvent<HTMLInputElement>) => setMaxSkillRating(getNumber(e))} />

                <FormGroup>
                    <FormControlLabel control={<Checkbox checked={firstLineIsHeader} style={{ color: "#777684" }}
                        onChange={(_e, c) => setFirstLineIsHeader(c)} />} label="First Line in file is Header" />
                </FormGroup>

            </Box>

            <button onClick={save} className="btn dark">Save</button>
            <button onClick={cancel} className="btn light">Cancel</button>
        </>
    );
}
// tslint:enable:no-unused-variable
