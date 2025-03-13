import { makeStyles, tokens } from "@fluentui/react-components";

export const useStyles = makeStyles({
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
  