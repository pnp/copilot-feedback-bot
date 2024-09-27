import React from 'react';
import moment from 'moment';
import { deleteImportLog, loadFileTypeNames, loadImportLogs } from '../../api/ApiCalls';
import { FileUpload } from './FileUpload';
import { Button } from '@mui/material';
import { GenerateStats } from './GenerateStats';
import { ConfirmDialogue } from '../../components/common/controls/ConfirmDialogue';
import { ImportConfigurations } from './ImportConfigurations';

export const Imports: React.FC<{ token: string, client: Client }> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [logs, setLogs] = React.useState<ImportLog[]>([]);
  const [fileTypes, setFileTypes] = React.useState<string[] | null>([]);
  const [selectedLogIdToDelete, setSelectedLogIdToDelete] = React.useState<string | null>(null);


  const loadThingsFromServer = React.useCallback(() => {
    setLoading(true);
    loadImportLogs(props.token)
      .then(async response => {
        setLogs(response);
        setLoading(false);
      })
      .catch((ex: any) => {
        console.error(ex);
        setLoading(false);
      });

    loadFileTypeNames(props.token).then(r => setFileTypes(r));
  }, [props.token]);

  const deleteImport = React.useCallback((id: string) => {

    setLoading(true);
    deleteImportLog(props.token, id)
      .then(async () => {
        setSelectedLogIdToDelete(null);
        loadThingsFromServer();
      });
  }, [props.token, loadThingsFromServer]);

  React.useEffect(() => {
    loadThingsFromServer();

    window.initMainJs();
  }, [loadThingsFromServer]);

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Imports</h1>
          <p>See previous imports and start a new one here.</p>
        </div>
      </section>

      {selectedLogIdToDelete &&
        <ConfirmDialogue title='Delete Import?' onCancel={() => setSelectedLogIdToDelete(null)} onConfirm={() => deleteImport(selectedLogIdToDelete)}>
          <div>Are you sure you want to delete this import?</div>
        </ConfirmDialogue>
      }

      <ul id="tabs">
        <li className="logs active">Import Logs</li>
        <li className="upload">Upload</li>
        <li className="fileformats">File Formats</li>
        <li className="testdata">Generate Test Data</li>
      </ul>
      <ul id="tab">
        
        <li className="logs active">
          <section className="imports--btns">
            <button onClick={loadThingsFromServer} className="btn light" disabled={loading}>Refresh Logs</button>
          </section>

          {loading ?
            <div>Loading...</div>
            :
            <>
              <section className="imports--table nopad">
                <p className="logs"><strong>Total import logs:</strong> {logs.length}</p>
                <table className='table'>
                  <thead>
                    <tr>
                      <th>Imported On</th>
                      <th>New Data Points</th>
                      <th>Data Source</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {logs.map(l => {
                      return <tr key={l.id}>
                        <td>{moment(l.importedOn).format('yyyy-MM-DD HH:mm:ss')}</td>
                        <td>{l.newDataPoints.toLocaleString()}</td>
                        <td>{l.source?.name}</td>
                        <td><Button onClick={() => setSelectedLogIdToDelete(l.id)}>Delete</Button></td>
                      </tr>
                    })
                    }
                  </tbody>
                </table>

              </section>
            </>
          }
        </li>

        <li className="upload">
          <h3>Upload new Data</h3>
          <p>Select your file, specify the format and upload to include in the dataset.</p>
          {fileTypes &&
            <FileUpload token={props.token} fileFormats={fileTypes} />
          }
        </li>
        <li className="fileformats">
          <ImportConfigurations token={props.token} />
        </li>

        <li className="testdata">
          <GenerateStats token={props.token} />
        </li>
      </ul>
    </div>
  );
};
