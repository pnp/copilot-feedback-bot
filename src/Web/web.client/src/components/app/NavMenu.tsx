import { useHistory } from 'react-router-dom';
import { TabValue, TabList, Tab, SelectTabData, SelectTabEvent } from '@fluentui/react-components';
import { useState } from 'react';

export function NavMenu() {

  const [selectedValue, setSelectedValue] = useState<TabValue>("home");
  const history = useHistory();
  const onTabSelect = (_: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
    if (data.value === "home") {
      history.push('/tabhome');
    }
    else if (data.value === "adoption") {
      history.push('/adoption');
    }
    else if (data.value === "onboarding") {
      history.push('/onboarding');
    }
    else if (data.value === "admin") {
      history.push('/admin');
    }
  };

  return (
    <div className='nav'>
      <TabList selectedValue={selectedValue} onTabSelect={onTabSelect}>
        <Tab id="Home" value="home">
          Home
        </Tab>
        <Tab id="Onboarding" value="onboarding">
          Onboarding
        </Tab>
        <Tab id="Adoption" value="adoption">
          Adoption
        </Tab>
        <Tab id="Admin" value="admin">
          Administration
        </Tab>
      </TabList>
    </div>
  );

}
