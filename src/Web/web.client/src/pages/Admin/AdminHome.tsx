import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { TabValue, TabList, Tab, SelectTabData, SelectTabEvent } from '@fluentui/react-components';
import { useState } from 'react';
import { SurveyManagerPage } from './SurveyEdit/SurveyManagerPage';
import { TriggersPage } from './Triggers/TriggersPage';

export function AdminHome(props: { loader?: BaseAxiosApiLoader }) {

  const [selectedValue, setSelectedValue] = useState<TabValue>("survey");
  const onTabSelect = (_: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
  };

  return (
    <div>
      <h1>Administration Home</h1>

      <div style={{ display: 'flex' }}>
        <div style={{ minWidth: '200px' }}>
          <TabList selectedValue={selectedValue} onTabSelect={onTabSelect} vertical>
            <Tab id="Survey" value="survey">
              Survey Editor
            </Tab>
            <Tab id="Triggers" value="triggers">
              Triggers
            </Tab>
          </TabList>
        </div>
        <div>
          {selectedValue === "survey" && (
            <div>
              <SurveyManagerPage loader={props.loader} />
            </div>
          )}
          {selectedValue === "triggers" && (
            <div>
              <TriggersPage loader={props.loader} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
