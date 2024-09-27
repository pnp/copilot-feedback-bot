import { Button, Checkbox, FormControl, FormControlLabel, FormGroup, Grid, InputLabel, MenuItem, Select, SelectChangeEvent } from "@mui/material";
import React from "react";
import { startNewImport } from "../../api/ApiCalls";
import { SessionView } from "./SessionView";

export const FileUpload: React.FC<{ token: string, fileFormats: string[] }> = (props) => {

  const [fileSelected, setFileSelected] = React.useState<File | undefined>() // also tried <string | Blob>
  const [importingFile, setImportingFile] = React.useState<boolean>(false);
  const [fullImport, setFullImport] = React.useState<boolean>(false);
  const [fileFormat, setFileFormat] = React.useState<string | undefined>(undefined);
  const [importSession, setImportSession] = React.useState<ImportSession<SkillsResolutionResult> | null>(null);

  const handleImageChange = function (e: React.ChangeEvent<HTMLInputElement>) {
    const fileList = e.target.files;

    if (!fileList) return;

    setFileSelected(fileList[0]);
  };


  React.useEffect(() => {
    setFileFormat(props.fileFormats[0]);

  }, [props.fileFormats]);

  const handleFileFormatChange = (event: SelectChangeEvent) => {
    setFileFormat(event.target.value as string);
  };

  const uploadFileClick = function (e: React.MouseEvent<HTMLSpanElement, MouseEvent>) {

    if (fileSelected && fileFormat) {
      setImportingFile(true);
      startNewImport(props.token, fileSelected, fileFormat, fullImport).then((newSession: ImportSession<SkillsResolutionResult>) => {

        setImportingFile(false);
        setFileSelected(undefined);
        setImportSession(newSession);

      }).catch(() => setImportingFile(false));
    }
    e.preventDefault();
  };

  return (
    <>
      {importSession ?
        <>
          <SessionView sessionId={importSession.rowKey} token={props.token} />
        </>
        :
        <>
          <Grid container spacing={2}>
            <Grid item>
              <Button
                variant="contained"
                component="label"
              >
                <input
                  type="file"
                  multiple={false} hidden
                  onChange={handleImageChange}
                />
                Select File
              </Button>
            </Grid>
            <Grid item>
              <FormControl>

                <InputLabel id="demo-simple-select-label">Format</InputLabel>
                <Select
                  labelId="demo-simple-select-label"
                  id="demo-simple-select"
                  value={fileFormat ?? ""}
                  label="Age"
                  onChange={handleFileFormatChange}
                >
                  {props.fileFormats.map(format => {
                    return <MenuItem value={format} key={format}>{format}</MenuItem>
                  })}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
          <Grid>
            <Grid item>
              <FormGroup>
                <FormControlLabel control={<Checkbox checked={fullImport} style={{ color: "#777684" }}
                  onChange={(_e, c) => setFullImport(c)} />} label="Full import (employees not in this file are assumed to have left the company)" />
              </FormGroup>
            </Grid>

            <Grid item>
              <Button disabled={!fileSelected || importingFile}
                component="span"
                variant="contained"
                onClick={uploadFileClick}>Upload File</Button>
            </Grid>
          </Grid>
          <div>
            {importingFile &&
              <span>Importing file...this might take a while if there's a lot of data...</span>
            }
          </div>
        </>
      }
    </>
  );
};
