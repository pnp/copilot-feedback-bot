
import React from 'react';
import 'chartjs-adapter-date-fns'
import { SkillsTree, TreeData } from '../../components/common/controls/SkillsTree';
import { Button, Typography } from '@mui/material';
import { Box } from '@mui/system';
import Modal from '@mui/material/Modal';

interface Props{
  skillsTreeData: TreeData[], 
  selectedSkillFilter: TreeData | null, 
  setselectedSkillFilter : Function,
  openFilterModal: boolean,
  handleOpen : Function,
  handleClose: Function
}

export const Filters: React.FC<Props> = (props) => {
  const handleClose = () => props.handleClose();
  const modalBoxStyle = {
    position: 'absolute' as 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    width: 400,
    bgcolor: 'white',
    border: '2px solid #000',
    boxShadow: 24,
    p: 4,
  };

  return (
    <div>

      <Modal
        open={props.openFilterModal}
        onClose={handleClose}
        aria-labelledby="modal-modal-title"
        aria-describedby="modal-modal-description"
      >
        <Box sx={modalBoxStyle}>
          <Typography id="modal-modal-title" variant="h6" component="h2">
            Filter on Skills
          </Typography>
          <SkillsTree treeData={props.skillsTreeData} disableDrag={true} itemClicked={(t: TreeData) => props.setselectedSkillFilter(t)} />

          {props.selectedSkillFilter ?
            <>
              <div>Selected: {props.selectedSkillFilter.name}</div>
              <Button onClick={() => props.setselectedSkillFilter(null)}>Clear</Button>
            </>
            :
            <>No filter selected</>
          }
        </Box>
      </Modal>

    </div>
  );
};
