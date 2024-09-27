import * as React from 'react';
import { MenuItem, Select, SelectChangeEvent } from '@mui/material';

export const LookupPicker: React.FC<PickerProps> = (props: PickerProps) => {

    const [selectedLookup, setSelectedLookup] = React.useState<AbstractEFEntityWithName | null>(null);
    const [filteredItems, setFilteredItems] = React.useState<AbstractEFEntityWithName[]>([]);

    const handleSelectedItemChange = (event: SelectChangeEvent) => {
        const selected = props.values.find(s => s.id === event.target.value);
        if (selected) {
            setSelectedLookup(selected)
            props.onChange(selected);
        }
    };

    // Only show items not filtered out
    React.useEffect(() => {
        const filteredItems = props.values.filter(v => (props.exclude.map(e => e.id).includes(v.id) === false));
        setFilteredItems(filteredItems);
    }, [props.exclude, props.values]);

    // Set selected back to "select" if selected item isn't in the list any more
    React.useEffect(() => {
        if (selectedLookup && !filteredItems.map(i => i.id).includes(selectedLookup.id)) {
            setSelectedLookup(null);
        }
    }, [filteredItems, selectedLookup]);

    return <Select
        value={selectedLookup?.id.toString() ?? "-1"}
        onChange={handleSelectedItemChange}
    >
        <MenuItem selected={true} value="-1">--SELECT--</MenuItem>
        {filteredItems.map(s => {
            if (props.labelOverride)
                return <MenuItem value={s.id}>{props.labelOverride(s)}</MenuItem>
            else
                return <MenuItem value={s.id}>{s.name}</MenuItem>
        })}
    </Select>
};

interface PickerProps {
    values: AbstractEFEntityWithName[];
    exclude: AbstractEFEntityWithName[];
    onChange: (newValue: AbstractEFEntityWithName) => void;
    labelOverride?: (val: AbstractEFEntityWithName) => string;
}
