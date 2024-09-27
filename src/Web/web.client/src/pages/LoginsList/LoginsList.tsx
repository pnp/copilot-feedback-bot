
import React from 'react';
import { loadFromApi } from '../../api/ApiLoader';
import { useHistory } from 'react-router-dom';

export const LoginsList: React.FC<{ token: string, client: Client }> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [users, setUsers] = React.useState<User[]>([]);
  const history = useHistory();

  const loadUsers = () => {
    setLoading(true);
    loadFromApi('Users/List', 'GET', props.token)
      .then(async response => {
        const data: User[] = JSON.parse(response);
        setUsers(data);
        setLoading(false);
      });
  }

  React.useEffect(() => {
    loadUsers();
    // eslint-disable-next-line
  }, []);

  const resubmitInvite = (user: User) => {

    user.inviting = true;
    user.inviteSent = false;
    setUsers(users.map(u => u));

    loadFromApi('Users/ResendInvite', 'POST', props.token, { emailAddress: user.emailAddress }, (dataText: string) => {
      alert('Could not resend invite. Errors:\n' + dataText);
    })
      .then(() => {
        user.inviteSent = true;
        setUsers(users.map(u => u));
        setTimeout(function () {
          user.inviteSent = false;
          user.inviting = false;
          loadUsers();
        }, 5000);
      })
      .catch(() => {
        user.inviting = false;
        setUsers(users.map(u => u));
      });
  }

  const goToInvite = () => {
    history.push('/loginsInvite');
  }

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Users</h1>
          <p>See a list of users with access to the analytics.</p>
          <button onClick={goToInvite} className="btn">Invite new user</button>
        </div>
      </section>

      {loading ?
        <div>Loading users...</div>
        :
        <>
          <section className="imports--table nopad">
            <p className="logs"><strong>Total Users:</strong> {users.length}</p>
            <table className='table'>
              <thead>
                <tr>
                  <th>Email Address</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {users.map(u => {
                  return <tr key={u.emailAddress}>
                    <td>{u.emailAddress}</td>
                    <td>{u.status}</td>
                    <td>
                      {u.status === "Pending" &&
                        <div>
                          <button onClick={() => resubmitInvite(u)} disabled={u.inviting}>Resend Invite</button>
                          {u.inviteSent &&
                            <span style={{ marginLeft: 20, color: 'green' }}>User invite has been sent.</span>
                          }
                        </div>
                        }
                    </td>
                  </tr>
                })
                }
              </tbody>
            </table>
          </section>
        </>
      }
    </div>
  );
};
