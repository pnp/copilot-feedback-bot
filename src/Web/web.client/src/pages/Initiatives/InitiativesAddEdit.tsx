import React, { ChangeEvent } from 'react';
import { addEditSkillsInitiative, loadJobTitles, loadSkillsTree } from '../../api/ApiCalls';
import { Button, Grid, InputLabel, TextField } from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { Moment } from 'moment';
import moment from 'moment';
import { LookupPicker } from '../../components/common/controls/LookupPicker';

export const InitiativesAddEdit: React.FC<{ token: string, client: Client, existing?: ClientSkillsInitiative, initiativeSavedCallback: Function, cancelCallback: Function }>
  = (props) => {

    const [jobTitleDefinitions, setJobTitleDefinitions] = React.useState<JobTitle[]>([]);
    const [jobTitleDefinitionCounts, setJobTitleDefinitionCounts] = React.useState<LookupWithEmployeeCount<JobTitle>[]>([]);
    const [skillNameDefinitions, setSkillNameDefinitions] = React.useState<SkillName[]>([]);

    const [editModeAddedSkills, setEditModeAddedSkills] = React.useState<SkillSelection[]>([]);
    const [editModeSelectedJobTitles, setEditModeSelectedJobTitles] = React.useState<JobTitle[]>([]);

    const [initiativeTitle, setInitiativeTitle] = React.useState<string>(props.existing?.name ?? "New initiative");
    const [initiativeCost, setInitiativeCost] = React.useState<number>(props.existing?.cost ?? 0);
    const [initiativeStartDate, setInitiativeStartDate] = React.useState<Moment | null>(null);
    const [initiativeEndDate, setInitiativeEndDate] = React.useState<Moment | null>(null);

    const [newSkillTargetLevel, setNewSkillTargetLevel] = React.useState<number>(0);
    const [newSkillToAdd, setNewSkillToAdd] = React.useState<SkillName | null>(null);

    const [newJobTitleToAdd, setNewJobTitleToAdd] = React.useState<JobTitle | null>(null);

    // Load events
    const refreshData = React.useCallback(() => {
      loadJobTitles(props.token).then(async response => {
        setJobTitleDefinitionCounts(response);
        setJobTitleDefinitions(response.map(r => r.item));
      });
      loadSkillsTree(props.token).then(async response => setSkillNameDefinitions(response));
    }, [props.token]);


    const getJobTitleCount = React.useCallback((j: JobTitle) => {
      const def = jobTitleDefinitionCounts.find(d=> d.item.id === j.id);
      return `${def?.item.name} (${def?.employeeCount})`;
    }, [jobTitleDefinitionCounts]);

    React.useEffect(() => {
      refreshData();

      // Set form vars
      if (props.existing?.start) {
        setInitiativeStartDate(moment(props.existing?.start))
      }
      if (props.existing?.end) {
        setInitiativeEndDate(moment(props.existing?.end))
      }
      if (props.existing?.targetAudiences)
        setEditModeSelectedJobTitles(props.existing.targetAudiences.map(a => a.jobTitle));
      if (props.existing?.targetSkills)
        setEditModeAddedSkills(props.existing.targetSkills.map(s => { return { skill: s.skill, target: s.targetAverage } }));
    }, [refreshData, props.existing]);


    const cancel = React.useCallback(() => {
      props.cancelCallback();
    }, [props]);

    const save = React.useCallback(() => {
      if (initiativeStartDate && initiativeEndDate) {
        const req: CreateEditInitiativeRequest =
        {
          cost: initiativeCost,
          start: initiativeStartDate.toDate(),
          end: initiativeEndDate.toDate(),
          name: initiativeTitle,
          id: props.existing?.id,
          targetAudiences: editModeSelectedJobTitles.map(d => { return { jobTitleId: d.id } }),
          targetSkills: editModeAddedSkills.map(d => { return { skillId: d.skill.id, targetAverage: d.target } })
        };

        addEditSkillsInitiative(props.token, req).then(() => {
          props.initiativeSavedCallback();
        });
      }
      else
        alert('Fill out required info');
    }, [initiativeStartDate, initiativeEndDate, initiativeTitle, initiativeCost, editModeAddedSkills, editModeSelectedJobTitles, props]);

    const getNumber = React.useCallback((e: ChangeEvent<HTMLInputElement>): number => {
      const value = !Number.isNaN(e.target.valueAsNumber) ? e.target.valueAsNumber : null;
      return (value ?? 0);
    }, []);

    // Add/remove skills & job-titles
    const addSelectedSkill = React.useCallback(() => {
      if (newSkillToAdd) {
        setEditModeAddedSkills((prev) => [...prev, { skill: newSkillToAdd, target: newSkillTargetLevel }])
      }
      else alert('Pick a skill to add');
    }, [newSkillToAdd, newSkillTargetLevel]);

    const addSelectedJobTitle = React.useCallback(() => {
      if (newJobTitleToAdd) {
        setEditModeSelectedJobTitles((prev) => [...prev, newJobTitleToAdd])
      }
      else alert('Pick a job-title to add');
    }, [newJobTitleToAdd]);

    const removeJobTitle = (id: string) => {
      const allExcept = editModeSelectedJobTitles.filter(s => s.id !== id);
      if (allExcept) {
        setEditModeSelectedJobTitles(allExcept);
      }
    };
    const removeSkillTarget = (id: string) => {
      const allExcept = editModeAddedSkills.filter(s => s.skill.id !== id);
      if (allExcept) {
        setEditModeAddedSkills(allExcept);
      }
    };

    const newTo = (dt: Moment | null) => {
      if (dt) {
        setInitiativeEndDate(dt);
      }
    }
    const newFrom = (dt: Moment | null) => {
      if (dt) {
        setInitiativeStartDate(dt);
      }
    }

    return (
      <div>
        {props.existing ?
          <h1>Edit Skills Initiative</h1>
          :
          <h1>New Skills Initiative</h1>
        }
        <Grid container>
          <Grid>
            <TextField label="Title" size='small'
              type="text" value={initiativeTitle}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setInitiativeTitle(e.target.value)} />
          </Grid>
          <Grid>
            <DatePicker
              label="Start"
              value={initiativeStartDate}
              onChange={newFrom}
            />
          </Grid>
          <Grid>
            <DatePicker
              label="End"
              value={initiativeEndDate}
              onChange={newTo}
            />
          </Grid>
          <Grid>
            <TextField label="Cost"
              type="number" value={initiativeCost}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setInitiativeCost(getNumber(e))} />
          </Grid>
        </Grid>

        <h3>For Job Titles</h3>
        {editModeSelectedJobTitles.length > 0 ?
          <table className='table'>
            {editModeSelectedJobTitles.map(s => {
              return <tr><td>{s.name}</td><td><Button onClick={() => removeJobTitle(s.id)}>Remove</Button></td></tr>
            })}
          </table>
          :
          <table className='table'>
            <tr><td>No job-title targets added</td></tr>
          </table>
        }
        <Grid container>
          <Grid><InputLabel>Add new job title target:</InputLabel></Grid>
          <Grid>
            <LookupPicker values={jobTitleDefinitions} exclude={editModeSelectedJobTitles}
              onChange={d => setNewJobTitleToAdd(d)} labelOverride={getJobTitleCount} />
          </Grid>
          <Grid>
            <Button onClick={addSelectedJobTitle}>Add</Button>
          </Grid>
        </Grid>

        <h3>Skills and Targets</h3>
        {editModeAddedSkills.length > 0 ?
          <table className='table'>
            {editModeAddedSkills.map(s => {
              return <tr><td>{s.skill.name}</td><td>{s.target}</td><td><Button onClick={() => removeSkillTarget(s.skill.id)}>Remove</Button></td></tr>
            }
            )}
          </table>
          :
          <table className='table'>
            <tr><td>No skill targets added</td></tr>
          </table>
        }

        <Grid container>
          <Grid><InputLabel>Add new skill target:</InputLabel></Grid>
          <Grid>
            <LookupPicker values={skillNameDefinitions} exclude={editModeAddedSkills.map(s => s.skill)}
              onChange={d => setNewSkillToAdd(d as SkillName)} />
          </Grid>
          <Grid>
            <TextField label="Target Value"
              type="number" value={newSkillTargetLevel}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setNewSkillTargetLevel(getNumber(e))} />
          </Grid>
          <Grid>
            <Button onClick={addSelectedSkill}>Add</Button>
          </Grid>
        </Grid>

        <button onClick={cancel} className="btn light">Cancel</button>
        <button onClick={save} className="btn">Save Changes</button>

      </div >
    );
  };

interface SkillSelection {
  skill: SkillName;
  target: number;
}