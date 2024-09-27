import * as React from 'react';

// Not needed?  https://mui.com/material-ui/react-tooltip/#custom-child-element
export const DivForTooltip = React.forwardRef<HTMLDivElement, React.PropsWithChildren<{}>>((props, ref) => (
    //  Spread the props to the underlying DOM element.
      <div {...props} ref={ref}>
        {props.children}
      </div>
  ));
  
