import React from "react";
import { Select, MenuItem, SelectChangeEvent } from "@mui/material";

export const ImportResultsSuggestion: React.FC<{ suggestion: SkillSuggestions, newSelection: Function }> = (props) => {
  const [selectedSuggestion, setSelectedSuggestion] = React.useState<string | undefined>(props.suggestion.suggestions[0]);

  const handleFileFormatChange = (event: SelectChangeEvent) => {
    const newSelection = event.target.value as string
    setSelectedSuggestion(newSelection);
    props.newSelection(newSelection);
  };

  return (
    <table>
      <tbody>
        <tr>
          <td>Original:</td><td>Suggestions:</td>
        </tr>
        <tr>
          <td>"{props.suggestion.original}"</td>
          <td>
            <Select
              value={selectedSuggestion ?? ""}
              label="Age"
              onChange={handleFileFormatChange}
            >
              {props.suggestion.suggestions.map(s => {
                return <MenuItem value={s} key={s}>{s}</MenuItem>
              })}
            </Select>
          </td>
        </tr>
      </tbody>
    </table>
  );
};
