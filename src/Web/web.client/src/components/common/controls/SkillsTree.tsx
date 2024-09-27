import React, { useCallback } from 'react';
import { MoveHandler, NodeApi, NodeRendererProps, Tree } from "react-arborist";
import { AiTwotoneTag, AiTwotoneTags } from "react-icons/ai";
import { MdArrowDropDown, MdArrowRight } from "react-icons/md";
import clsx from "clsx";
import styles from "./skills.module.css";

export interface TreeData {
  id: string,
  name: string,
  children: TreeData[]
}

export const SkillsTree: React.FC<{ treeData: TreeData[], disableDrag?: boolean, itemClicked?: Function, itemMoved?: Function }> = (props) => {

  const setActive = useCallback((t: TreeData) => {
    if (props.itemClicked) {
      props.itemClicked(t);
    }
  }, [props]);


  const onMove = (args: MoveHandler<TreeData>): void => {
    console.log(args);
    if (props.itemMoved) {
      const dArgs: any = args;
      const e: MoveNodeParams = { skillId: dArgs.dragIds[0], newParentId: dArgs.parentId };
      props.itemMoved(e);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.treeContainer}>
        <>
          <Tree
            data={props.treeData}
            openByDefault={true}
            className={styles.tree}
            disableDrag={props.disableDrag}
            rowClassName={styles.row}
            padding={15}
            rowHeight={30}
            onActivate={(node) => setActive(node.data)}
            indent={INDENT_STEP}
            overscanCount={8}
            onMove={(args: any) => onMove(args)}
          >
            {Node}
          </Tree>
        </>
      </div>
    </div>
  );
};

export const convertSkillsToTreeNode = (skills: SkillName[]): TreeData[] => {
  let d: TreeData[] = [];

  addToList(skills, d);

  return d;
}


function addToList(skillsResponse: SkillName[], treeArray: TreeData[]) {
  skillsResponse.forEach(s => {

    const treeNode: TreeData = { id: String(s.id), name: s.name, children: [] };
    treeArray.push(treeNode);
    addToList(s.children, treeNode.children!)
  });
}
const INDENT_STEP = 15;

function Node({ node, style, dragHandle }: NodeRendererProps<TreeData>) {
  const Icon = node.isInternal ? AiTwotoneTags : AiTwotoneTag;
  const indentSize = Number.parseFloat(`${style.paddingLeft || 0}`);

  return (
    <div
      ref={dragHandle}
      style={style}
      className={clsx(styles.node, node.state)}
      onClick={() => node.isInternal && node.toggle()}
    >
      <div className={styles.indentLines}>
        {new Array(indentSize / INDENT_STEP).fill(0).map((_, index) => {
          return <div key={index}></div>;
        })}
      </div>
      <FolderArrow node={node} />
      <Icon className={styles.icon} />{" "}
      <span className={styles.text}>
        {node.isEditing ? <Input node={node} /> : node.data.name}
      </span>
    </div>
  );
}

function Input({ node }: { node: NodeApi<TreeData> }) {
  return (
    <input
      autoFocus
      name="name"
      type="text"
      defaultValue={node.data.name}
      onFocus={(e) => e.currentTarget.select()}
      onBlur={() => node.reset()}
      onKeyDown={(e) => {
        if (e.key === "Escape") node.reset();
        if (e.key === "Enter") node.submit(e.currentTarget.value);
      }}
    />
  );
}

function FolderArrow({ node }: { node: NodeApi<TreeData> }) {
  return (
    <span className={styles.arrow}>
      {node.isInternal && node.children && node.children.length > 0 ? (
        node.isOpen ? (
          <MdArrowDropDown />
        ) : (
          <MdArrowRight />
        )
      ) : null}
    </span>
  );
}

export interface MoveNodeParams {
  skillId: string,
  newParentId?: string
}
