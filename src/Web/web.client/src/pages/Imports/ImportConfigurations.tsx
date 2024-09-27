import React from "react";
import { deleteImportConfigurations, loadImportConfigurations, saveImportConfigurations } from "../../api/ApiCalls";
import { IconButton } from "@mui/material";
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import AddIcon from '@mui/icons-material/Add';
import { ImportConfigurationsAddEdit } from "./ImportConfigurationsAddEdit";
import { PageViewMode } from "../../apimodels/Enums";
import { ConfirmDialogue } from "../../components/common/controls/ConfirmDialogue";

export const ImportConfigurations: React.FC<{ token: string }> = (props) => {
    const [viewMode, setViewMode] = React.useState<PageViewMode>(PageViewMode.View);
    const [loading, setLoading] = React.useState<boolean>(false);
    const [selectedConfigIdToDelete, setSelectedConfigIdToDelete] = React.useState<string | null>(null);
    const [selectedImportConfiguration, setSelectedImportConfiguration] = React.useState<ImportConfiguration | undefined>(undefined);
    const [importConfigurations, setImportConfigurations] = React.useState<ImportConfiguration[]>([]);

    const loadThingsFromServer = React.useCallback(() => {
        setLoading(true);

        loadImportConfigurations(props.token).then(r => {
            setImportConfigurations(r);
            setLoading(false);
        });
    }, [props.token]);

    React.useEffect(() => {
        loadThingsFromServer();
    }, [loadThingsFromServer]);

    const startEditOrNew = React.useCallback((c?: ImportConfiguration) => {
        setSelectedImportConfiguration(c);
        if (c) {
            setViewMode(PageViewMode.Edit);
        }
        else
            setViewMode(PageViewMode.New);
    }, []);

    const saveConfig = React.useCallback((config: ImportConfiguration) => {
        setLoading(true);
        saveImportConfigurations(props.token, config).then(() => {
            loadThingsFromServer();
            setViewMode(PageViewMode.View);
        });
    }, [loadThingsFromServer, props.token]);


    const cancelEdit = React.useCallback(() => {
        setSelectedImportConfiguration(undefined);
        setViewMode(PageViewMode.View);
    }, []);

    React.useEffect(() => {
        loadThingsFromServer();
    }, [loadThingsFromServer]);


    const deleteImportConfig = React.useCallback((id: string) => {

        setLoading(true);
        deleteImportConfigurations(props.token, id)
            .then(async () => {
                setSelectedConfigIdToDelete(null);
                loadThingsFromServer();
            });
    }, [props.token, loadThingsFromServer]);


    return (
        <>
            {viewMode === PageViewMode.View ?
                <>
                    <h3>Existing Formats</h3>
                    <p>These are the formats you can import skills data with. </p>
                    {selectedConfigIdToDelete &&
                        <ConfirmDialogue title='Delete Config?' onCancel={() => setSelectedConfigIdToDelete(null)} 
                                onConfirm={() => deleteImportConfig(selectedConfigIdToDelete)}>
                            <div>Are you sure you want to delete this import configuration?</div>
                        </ConfirmDialogue>
                    }

                    {loading ?
                        <div>Loading...</div>
                        :
                        <section className="imports--table nopad">
                            <table className='table'>
                                <thead>
                                    <tr>
                                        <th>Configuration Name</th>
                                        <th>
                                            <IconButton aria-label="new" color="primary" onClick={() => startEditOrNew(undefined)}>
                                                <AddIcon />
                                            </IconButton>
                                        </th>
                                        <th />
                                    </tr>
                                </thead>
                                <tbody>
                                    {importConfigurations.map((c: ImportConfiguration) => {
                                        return <tr key={c.id}>
                                            <td>{c.name}</td>
                                            <td>
                                                <IconButton aria-label="delete" color="primary" onClick={() => setSelectedConfigIdToDelete(c.id)}>
                                                    <DeleteIcon />
                                                </IconButton>
                                            </td>
                                            <td>
                                                <IconButton aria-label="edit" color="primary" onClick={() => startEditOrNew(c)}>
                                                    <EditIcon />
                                                </IconButton>
                                            </td>
                                        </tr>
                                    })
                                    }
                                </tbody>
                            </table>
                        </section>
                    }
                </>
                :
                <>
                    <ImportConfigurationsAddEdit model={selectedImportConfiguration}
                        saveCallback={(c: ImportConfiguration) => saveConfig(c)}
                        cancelCallback={cancelEdit} />
                </>
            }

        </>
    );
}
