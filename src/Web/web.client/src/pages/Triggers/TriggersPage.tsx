
import React from 'react';
import 'chartjs-adapter-date-fns'
import { BaseApiLoader } from '../../api/ApiLoader';
import { Button, Input, Spinner } from '@fluentui/react-components';
import { triggerGenerateFakeActivityForUser, triggerInstallBotForUser, triggerSendSurveys } from '../../api/ApiCalls';

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

export const TriggersPage: React.FC<{ loader?: BaseApiLoader }> = (props) => {

  const sendSurveys = React.useCallback(() => {
    console.debug("Sending Surveys");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    setLoading(true);
    triggerSendSurveys(props.loader).then(() => { setLoading(false); });
  }, []);

  const installBot = React.useCallback(() => {  
    console.debug("Installing Bot");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    triggerInstallBotForUser(props.loader, installUser).then(() => { setLoading(false); });
  }, []);

  const generateData = React.useCallback(() => {
    console.debug("Generating Data");
    if (!props.loader) {
      console.error("No loader available");
      return;
    }
    triggerGenerateFakeActivityForUser(props.loader, generateDataUser).then(() => { setLoading(false); });
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
                  <Input placeholder="UPN" onChange={(e)=> setInstallUser(e.currentTarget.value)} value={installUser} disabled={loading} />
                </TableCell>
                <TableCell>
                  <Button appearance="primary" onClick={installBot} disabled={loading}>Trigger Install</Button>
                </TableCell>
              </TableRow>
              
              <TableRow>
                <TableCell>
                  <TableCellLayout media={<Play24Regular />}>Generate fake copilot activity for user</TableCellLayout>
                </TableCell>
                <TableCell>
                  <Input placeholder="UPN" onChange={(e)=> setGenerateDataUser(e.currentTarget.value)} value={generateDataUser} disabled={loading} />
                </TableCell>
                <TableCell>
                  <Button appearance="primary" onClick={generateData} disabled={loading}>Generate Data</Button>
                </TableCell>
              </TableRow>

            </TableBody>
          </Table>

          
          {loading && <Spinner label="Sending command..." />}

        </div >
      </section >

    </div >
  );
};
