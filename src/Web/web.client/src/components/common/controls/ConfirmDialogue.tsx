import * as React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { PropsWithChildren } from 'react';

export const ConfirmDialogue: React.FC<PropsWithChildren<{ title: string, onConfirm: Function, onCancel: Function }>> = (props) => {

    const handleClose = (confirm: boolean) => {
        if (confirm) {
            props.onConfirm();
        }
        else
            props.onCancel();
    };

    return (
        <Dialog
            open={true}
            onClose={handleClose}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">
                {props.title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    {props.children}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button onClick={() => handleClose(false)}>No</Button>
                <Button onClick={() => handleClose(true)} autoFocus>Yes</Button>
            </DialogActions>
        </Dialog>
    );
}