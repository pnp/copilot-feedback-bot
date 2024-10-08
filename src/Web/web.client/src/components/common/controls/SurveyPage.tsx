import React from "react";
import { SurveyPageDTO } from "../../../apimodels/Models";
import { Card, CardHeader, makeStyles, Text, tokens } from "@fluentui/react-components";
import { AdaptiveCard } from "./AdaptiveCard";

const useStyles = makeStyles({
  main: {
    gap: "36px",
    display: "flex",
    flexDirection: "column",
    flexWrap: "wrap",
  },

  card: {
    width: "360px",
    maxWidth: "100%",
    height: "fit-content",
  },

  section: {
    width: "fit-content",
  },

  title: { margin: "0 0 12px" },

  horizontalCardImage: {
    width: "64px",
    height: "64px",
  },

  headerImage: {
    borderRadius: "4px",
    maxWidth: "44px",
    maxHeight: "44px",
  },

  caption: {
    color: tokens.colorNeutralForeground3,
  },

  text: { margin: "0" },
});


export const SurveyPage: React.FC<{ page: SurveyPageDTO }> = (props) => {
  const styles = useStyles();

  return (
    <div className="adaptiveCardContainer">
      <Card className={styles.card}>
        <CardHeader
          header={<Text weight="semibold">{props.page.name}</Text>}
        />
        <AdaptiveCard json={props.page.adaptiveCardTemplateJson} />
      </Card>
    </div>
  );
};
