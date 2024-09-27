
import React, { ChangeEvent } from 'react';
import { loadJobTitles, loadSkillsTree } from '../../api/ApiCalls';
import moment, { Moment } from 'moment';
import { DateRangePicker } from '../../components/common/controls/DateRangePicker';
import { LoadingSpinner } from '../../components/common/controls/LoadingSpinner';
import { Stack, Slider, TextField } from '@mui/material';
import { loadFromApi } from '../../api/ApiLoader';

import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';

enum DataType {
  Unknown,
  SkillNames,
  JobTitles
}

export const GenerateStats: React.FC<{ token: string }> = (props) => {

  const [selectedFrom, setSelectedFrom] = React.useState<Moment>(moment(new Date()).add(-7, 'days'));
  const [selectedTo, setSelectedTo] = React.useState<Moment>(moment(new Date()));

  const [skillsByJobTitle, setSkillsByJobTitle] = React.useState<GenerateSkillsDataRequestByDemographic[]>();
  const [skillsBySkillName, setSkillsBySkillName] = React.useState<GenerateSkillsDataRequestByDemographic[]>();

  const [skillsByJobTitleAllVal, setSkillsByJobTitleAllVal] = React.useState<number>();
  const [skillsBySkillNameAllVal, setSkillsBySkillNameAllVal] = React.useState<number>();

  const [generating, setGenerating] = React.useState<boolean>(false);
  const [insertsWanted, setInsertsWanted] = React.useState<number>(20);

  const refreshData = React.useCallback(() => {

    loadJobTitles(props.token)
      .then(async response => {
        setSkillsByJobTitle(response.map(d => {
          return {
            name: d.item.name,
            skillLevel: 4
          } as GenerateSkillsDataRequestByDemographic
        }));
      });

    loadSkillsTree(props.token).then(async response => {
      setSkillsBySkillName(response.map(d => {
        return {
          name: d.name,
          skillLevel: 4
        } as GenerateSkillsDataRequestByDemographic
      }));
    });
  }, [props.token]);

  React.useEffect(() => {
    refreshData();
  }, [refreshData]);

  const newDatesSelected = (from: Moment, to: Moment) => {
    setSelectedFrom(from);
    setSelectedTo(to);
  }

  const submitGenerationRequest = (type: DataType) => {

    const from = selectedFrom ?? new Date();
    const to = selectedTo ?? new Date();

    if (from && to) {
      const options: GenerateDataRequest =
      {
        from: from.toDate(),
        to: to.toDate(),
        skills: type === DataType.JobTitles ? skillsByJobTitle! : skillsBySkillName!,
        testDataType: type,
        inserts: insertsWanted
      };
      setGenerating(true);
      loadFromApi('Stats/GenerateStats', 'POST', props.token, options)
        .then(() => {
          setGenerating(false);
        })
        .catch(() => {
          setGenerating(false);
        });
    }
    else {
      alert('Invalid dates');
    }
  }


  const setJobTitleSkillValue = (index: number, value: number) => {
    if (skillsByJobTitle) {
      var sentiment = skillsByJobTitle![index];
      sentiment.skillLevel = value;
      setSkillsByJobTitle([...skillsByJobTitle])
    }
  }
  const setJobTitleSkillValueAll = (value: number) => {
    setSkillsByJobTitleAllVal(value);
    if (skillsByJobTitle) {

      skillsByJobTitle.forEach((_j, i) => {
        var s = skillsByJobTitle[i];
        s.skillLevel = value;
      });
      setSkillsByJobTitle([...skillsByJobTitle])
    }
  }

  const setSkillNameeSkillValue = (index: number, value: number) => {
    if (skillsBySkillName) {
      var sentiment = skillsBySkillName[index];
      sentiment.skillLevel = value;
      setSkillsByJobTitle([...skillsBySkillName])
    }
  }
  const setSkillNameSkillValueAll = (value: number) => {
    setSkillsBySkillNameAllVal(value);
    if (skillsBySkillName) {

      skillsBySkillName.forEach((_j, i) => {
        var s = skillsBySkillName[i];
        s.skillLevel = value;
      });
      setSkillsBySkillName([...skillsBySkillName])
    }
  }


  interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
  }
  function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props;

    return (
      <div
        role="tabpanel"
        hidden={value !== index}
        id={`simple-tabpanel-${index}`}
        aria-labelledby={`simple-tab-${index}`}
        {...other}
      >
        {value === index && (
          <Box sx={{ p: 3 }}>
            <Typography>{children}</Typography>
          </Box>
        )}
      </div>
    );
  }

  function a11yProps(index: number) {
    return {
      id: `simple-tab-${index}`,
      'aria-controls': `simple-tabpanel-${index}`,
    };

  }

  const handleTabChange = (_e: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };
  const [tabValue, setTabValue] = React.useState(0);

  const onNumberChange = (e: ChangeEvent<HTMLInputElement>) => {
    // In general, use Number.isNaN over global isNaN as isNaN will coerce the value to a number first
    // which likely isn't desired
    const value = !Number.isNaN(e.target.valueAsNumber) ? e.target.valueAsNumber : null;

    setInsertsWanted(value ?? 0);
  }
  return (
    <div>
      <h3>Generate Skills Import</h3>
      <p>Pick to/from, and what data you want to generate.</p>
      <div style={{marginTop: 50}}>
        <DateRangePicker initialFrom={selectedFrom} intialTo={selectedTo} newDatesCallback={(from: Moment, to: Moment) => newDatesSelected(from, to)} />

        <TextField type='number' onChange={onNumberChange} label='Number of skills to insert' value={insertsWanted} />
      </div>
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={handleTabChange} aria-label="basic tabs example">
          <Tab label="By Skill" {...a11yProps(0)} />
          <Tab label="By Job Title" {...a11yProps(1)} />
        </Tabs>
      </Box>
      <TabPanel value={tabValue} index={0}>

        <p>Generate fake skill data for employee with which skill-names. For each day, add a configurable number of skill ratings to rando people:</p>
        {!skillsBySkillName ?
          <p><LoadingSpinner /></p>
          :
          <>
            <Stack>
              <p>All:</p>
              <Slider
                aria-label="Skill level All"
                defaultValue={4}
                valueLabelDisplay="auto"
                onChange={(_, value) => setSkillNameSkillValueAll(value as number)}
                step={1}
                marks
                min={0}
                max={8}
                color='primary'
                value={skillsBySkillNameAllVal}
              />
            </Stack>
            <Stack spacing={3}>
              {skillsBySkillName.map((jobTitleConfig, index) => {
                return <div>
                  <p>{jobTitleConfig.name} - {jobTitleConfig.skillLevel}</p>

                  {skillsBySkillName.length > 0 &&
                    <Slider
                      aria-label={"" + jobTitleConfig.skillLevel}
                      value={jobTitleConfig.skillLevel}
                      valueLabelDisplay="auto"
                      onChange={(_, value) => setSkillNameeSkillValue(index, value as number)}
                      step={1}
                      min={0}
                      max={8}
                      color='secondary'
                    />
                  }

                </div>
              })}
            </Stack>
          </>
        }

        {!generating &&
          <p>
            <button onClick={() => submitGenerationRequest(DataType.SkillNames)} className="btn MuiButtonBase-root MuiButton-root MuiButton-text MuiButton-textPrimary MuiButton-sizeMedium MuiButton-textSizeMedium css-1ujsas3">Generate Activity Data</button>
          </p>
        }
      </TabPanel>

      <TabPanel value={tabValue} index={1}>

        <p>Generate fake skill data for employee with which job titles:</p>
        {!skillsByJobTitle ?
          <p><LoadingSpinner /></p>
          :
          <>
            <Stack>
              <p>All:</p>
              <Slider
                aria-label="Skill level All"
                defaultValue={4}
                valueLabelDisplay="auto"
                onChange={(_, value) => setJobTitleSkillValueAll(value as number)}
                step={1}
                marks
                min={0}
                max={8}
                value={skillsByJobTitleAllVal}
                color='primary'
              />
            </Stack>
            <Stack spacing={3}>
              {skillsByJobTitle.map((jobTitleConfig, index) => {
                return <div>
                  <p>{jobTitleConfig.name} - {jobTitleConfig.skillLevel}</p>

                  {skillsByJobTitle.length > 0 &&
                    <Slider
                      aria-label={"" + jobTitleConfig.skillLevel}
                      value={jobTitleConfig.skillLevel}
                      valueLabelDisplay="auto"
                      onChange={(_, value) => setJobTitleSkillValue(index, value as number)}
                      step={1}
                      min={0}
                      max={8}
                      color='secondary'
                    />
                  }

                </div>
              })}
            </Stack>
          </>
        }

        {!generating &&
          <p>
            <button onClick={() => submitGenerationRequest(DataType.JobTitles)} className="btn MuiButtonBase-root MuiButton-root MuiButton-text MuiButton-textPrimary MuiButton-sizeMedium MuiButton-textSizeMedium css-1ujsas3">Generate Activity Data</button>
          </p>
        }
      </TabPanel>


    </div>
  );
};

interface GenerateSkillsDataRequestByDemographic {
  name: string,
  skillLevel: number
}


interface GenerateDataRequest {
  from: Date,
  to: Date,
  skills: GenerateSkillsDataRequestByDemographic[],
  testDataType: DataType,
  inserts: number
}