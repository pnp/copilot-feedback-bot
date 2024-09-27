import React from "react";
import { getImportStatus } from "../../api/ApiCalls";
import { ImportResults } from "./ImportResults";
import { DateOnlyLabel } from "../../components/common/controls/DateOnlyLabel";

export const SessionView: React.FC<{ token: string, sessionId: string }> = (props) => {

  const [importSession, setImportSession] = React.useState<ImportSession<SkillsResolutionResult> | null>(null);

  // Refresh sesh
  React.useEffect(() => {
    console.log(`Initialising refresh interval`);
    const interval = setInterval(() => {
      getImportStatus(props.token, props.sessionId).then((s) => setImportSession(s));
    }, 5000);

    return () => {
      console.log(`Clearing interval`);
      clearInterval(interval);
    };
  }, [props.sessionId, props.token]); // has no dependency - this will be called on-component-mount

  return (
    <>
      {!importSession?.results ?
        <h4>Importing data...</h4>
        :
        <h4>All Done - Please Review Results</h4>
      }
      {importSession &&
        <>
          <table>
            <tbody>
              <tr>
                <td>Started:</td>
                <td>Finished:</td>
              </tr>
              <tr>
                <td><DateOnlyLabel val={importSession.fileImportStartedUtc} /></td>
                <td><DateOnlyLabel val={importSession.fileImportFinishedUtc} /></td>
                <td>{importSession.errors}</td>
              </tr>
              {importSession.results &&
                <>
                  <tr>
                    <td>Unresolved Skills ({importSession.results.unresolvedSkills.length}):</td>
                    <td>{importSession.results.unresolvedSkills.map(s=> <span>{s};</span>)}</td>
                  </tr>
                  <tr>
                    <td>{importSession.results.autoImportedSkills.length} skills imported with high confidence</td>
                    <td>{importSession.results.autoImportedSkills.map(s=> <span>{s.skillName};</span>)}</td>
                  </tr>
                </>
              }
            </tbody>
          </table>

          {importSession.results &&
            <ImportResults results={importSession.results} />
          }
        </>
      }
    </>
  );
};
