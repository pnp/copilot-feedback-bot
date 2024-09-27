
import { DatePicker } from '@mui/x-date-pickers';
import { Moment } from 'moment';
import React from 'react';

export const DateRangePicker: React.FC<{ initialFrom: Moment, intialTo: Moment, newDatesCallback: Function }> = (props) => {

  const [selectedFrom, setSelectedFrom] = React.useState<Moment>(props.initialFrom);
  const [selectedTo, setSelectedTo] = React.useState<Moment>(props.intialTo);

  const newTo = (dt: Moment | null) => {
    if (dt) {
      setSelectedTo(dt);
    }
  }
  const newFrom = (dt: Moment | null) => {
    if (dt) {
      setSelectedFrom(dt);
    }
  }

  const { newDatesCallback } = props;

  React.useEffect(() => {
    // Raise new dates event if valid
    if (selectedFrom < selectedTo) {
      newDatesCallback(selectedFrom, selectedTo);
    }
  }, [selectedFrom, selectedTo, newDatesCallback]);

  return (
    <span>
      <DatePicker disableFuture maxDate={selectedTo}
        label="From"
        value={selectedFrom}
        onChange={(newValue) => newFrom(newValue)}
      />

      <DatePicker disableFuture minDate={selectedFrom}
        label="To"
        value={selectedTo}
        onChange={(newValue) => {
          newTo(newValue)
        }}
      />
    </span>
  );
};
