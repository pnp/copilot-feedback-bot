import React from "react";
import { ImportResultsSuggestion } from "./ImportResultsSuggestion";

export const ImportResults: React.FC<{ results: SkillsResolutionResult }> = (props) => {

  const [changeSuggestions, setChangeSuggestions] = React.useState<SkillPick[]>(
    props.results.changeSuggestions.map(s => { return { original: s.original, picked: s.suggestions[0] } })
  );

  const newSelection = React.useCallback((newSelection: string, parent: SkillSuggestions) => {
    const n: SkillPick[] = changeSuggestions.map(s => {
      if (s.original === parent.original) {
        return { original: s.original, picked: newSelection };
      }
      else
        return s
    });
    setChangeSuggestions(n);

  }, [changeSuggestions]);

  return (
    <>
      {props.results.changeSuggestions.length > 0 ?
        <>
        <h4>Change suggestions</h4>
          {props.results.changeSuggestions.map(s => {
            return <ImportResultsSuggestion suggestion={s} newSelection={(newS: string) => newSelection(newS, s)} />
          })}
        </>
        :
        <div>No suggested skillname changes</div>
      }

    </>
  );
};
