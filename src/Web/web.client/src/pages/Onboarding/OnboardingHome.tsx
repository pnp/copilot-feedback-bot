import { BaseAxiosApiLoader } from '../../api/AxiosApiLoader';
import { TabValue, TabList, Tab, SelectTabData, SelectTabEvent } from '@fluentui/react-components';
import { useState } from 'react';


export function OnboardingHome(props: { loader?: BaseAxiosApiLoader }) {

  const [selectedValue, setSelectedValue] = useState<TabValue>("home");
  const onTabSelect = (_: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
  };

  return (
    <div>
      <h1>Onboarding</h1>
      <TabList selectedValue={selectedValue} onTabSelect={onTabSelect} vertical>
        <Tab id="Survey" value="survey">
          Survey Editor
        </Tab>
        <Tab id="Triggers" value="triggers">
          Triggers
        </Tab>
      </TabList>

      <div>
        {selectedValue === "survey" && (
          <div>
          </div>
        )}
        {selectedValue === "triggers" && (
          <div>
            Triggers
          </div>
        )}
      </div>
    </div>
  );
}
