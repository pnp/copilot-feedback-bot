import React from "react";
import * as AdaptiveCards from "adaptivecards";


export const AdaptiveCard: React.FC<{ json: string }> = (props) => {

  const [card, setCard] = React.useState<HTMLElement | null>(null);

  React.useEffect(() => {
    if (props.json) {
      const card = new AdaptiveCards.AdaptiveCard();
      card.parse(JSON.parse(props.json));
      setCard(card.render());
      card.render();
    }
  }, []);

  return (
    <div className="adaptiveCardContainer">
      {card &&
        <div ref={(n) => n && n.appendChild(card)}></div>
      }
    </div>
  );
};
