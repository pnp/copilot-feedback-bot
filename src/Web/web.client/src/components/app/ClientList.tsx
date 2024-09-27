
import React from 'react';
import { loadFromApi } from '../../api/ApiLoader';

export const ClientList: React.FC<{ token: string, clientSelected: Function }> = (props) => {

  const [clients, setClients] = React.useState<Client[] | null>();

  React.useEffect(() => {
    loadFromApi('Clients', 'GET', props.token)
      .then(async response => {
          const data: Client[] = JSON.parse(response);
          setClients(data);
        
      });
  }, [props.token]);

  return (
    <div>

      {!clients ?
        <p>Loading...</p>
        :
        <>
          {clients.map((client) => {
            return <p id={client.dnsLabel}><button onClick={()=> props.clientSelected(client)}>{client.name}</button></p>
          })
        }
        </>
      }
    </div>
  );
};
