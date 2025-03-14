
import React from 'react';
import 'chartjs-adapter-date-fns'
import { Button, Input, Spinner } from '@fluentui/react-components';
import { triggerGenerateFakeActivityForUser, triggerInstallBotForUser, triggerSendSurveys } from '../../../api/ApiCalls';

import {
  Play24Regular
} from "@fluentui/react-icons";
import {
  TableBody,
  TableCell,
  TableRow,
  Table,
  TableHeader,
  TableHeaderCell,
  TableCellLayout
} from "@fluentui/react-components";
import { BaseAxiosApiLoader } from '../../../api/AxiosApiLoader';

export const TriggersPage: React.FC<{ loader?: BaseAxiosApiLoader }> = (props) => {

  const [error, setError] = React.useState<string | undefined>(undefined);

  const sendSurveys = React.useCallback(() => {
    console.debug("Sending Surveys");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    setLoading(true);
    executeTrigger(triggerSendSurveys(props.loader));
  }, []);

  const installBot = React.useCallback(() => {
    console.debug("Installing Bot");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    executeTrigger(triggerInstallBotForUser(props.loader, installUser));
  }, []);

  const generateDataForUser = React.useCallback(() => {
    console.debug("Generating data for user");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    executeTrigger(triggerGenerateFakeActivityForUser(props.loader, generateDataUser));
  }, []);


  const generateDataForAllUsers = React.useCallback(() => {
    console.debug("Generating Data");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    executeTrigger(triggerGenerateFakeActivityForUser(props.loader, generateDataUser));
  }, []);

  const executeTrigger = React.useCallback((call: Promise<null>) => {
    console.debug("Executing trigger");
    setLoading(true);
    call
      .then(() => { setLoading(false); })
      .catch((err) => {
        console.error("Error executing trigger: ", err);
        setError(err.message);
        setLoading(false);
      });
  }, []);

  const [loading, setLoading] = React.useState<boolean>(false);
  const [installUser, setInstallUser] = React.useState<string>('');
  const [generateDataUser, setGenerateDataUser] = React.useState<string>('');

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Trigger an Action</h1>
          <p>You can control some actions of the system manually here.</p>

          <Table arial-label="Default table" style={{ minWidth: "510px" }}>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Action</TableHeaderCell>
                <TableHeaderCell>Parameters</TableHeaderCell>
                <TableHeaderCell>Trigger</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow>
                <TableCell>
                  <TableCellLayout media={<Play24Regular />}>Send bot surveys to all users with unsurveyed activities</TableCellLayout>
                </TableCell>
                <TableCell>--</TableCell>
                <TableCell>
                  <Button appearance="primary" onClick={sendSurveys} disabled={loading}>Send Surveys</Button>
                </TableCell>
              </TableRow>

              <TableRow>
                <TableCell>
                  <TableCellLayout media={<Play24Regular />}>Install bot for user</TableCellLayout>
                </TableCell>
                <TableCell>
                  <Input placeholder="UPN" onChange={(e) => setInstallUser(e.currentTarget.value)} value={installUser} disabled={loading} />
                </TableCell>
                <TableCell>
                  <Button appearance="primary" onClick={installBot} disabled={loading}>Trigger Install</Button>
                </TableCell>
              </TableRow>

              <TableRow>
                <TableCell>
                  <TableCellLayout media={<Play24Regular />}>Generate fake activity for user</TableCellLayout>
                </TableCell>
                <TableCell>
                  <Input placeholder="UPN" onChange={(e) => setGenerateDataUser(e.currentTarget.value)} value={generateDataUser} disabled={loading} />
                </TableCell>
                <TableCell>
                  <Button appearance="primary" onClick={generateDataForUser} disabled={loading}>Generate Data</Button>
                </TableCell>
              </TableRow>


              <TableRow>
                <TableCell>
                  <TableCellLayout media={<Play24Regular />}>Generate fake activity for all users</TableCellLayout>
                </TableCell>
                <TableCell>
                  --
                </TableCell>
                <TableCell>
                  <Button appearance="primary" onClick={generateDataForAllUsers} disabled={loading}>Generate Data</Button>
                </TableCell>
              </TableRow>

            </TableBody>
          </Table>

          {error && <div className="error">{error}</div>}

          {loading && <Spinner label="Sending command..." />}

        </div >
      </section >

    </div >
  );
};
