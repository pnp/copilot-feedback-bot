import React from 'react';
import { MoveNodeParams, SkillsTree, TreeData, convertSkillsToTreeNode } from '../../components/common/controls/SkillsTree';
import { loadSkillsTree, moveSkillNode } from '../../api/ApiCalls';

export const SkillsAdmin: React.FC<{ token: string }> = (props) => {

  const [skillsData, setSkillsData] = React.useState<TreeData[] | null>(null);

  const moveNode = (e: MoveNodeParams) => {
    moveSkillNode(props.token, e).then(async response => {
      setSkillsData(convertSkillsToTreeNode(response));
    });
  }

  React.useEffect(() => {
    // Demograph stats
    loadSkillsTree(props.token)
      .then(async response => {
        setSkillsData(convertSkillsToTreeNode(response));
      });
  }, [props.token]);

  

  return (
    <div>
      {skillsData &&
        <SkillsTree treeData={skillsData} disableDrag={false} itemMoved={moveNode} />
      }
    </div>
  );
};
