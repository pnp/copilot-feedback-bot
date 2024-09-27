
import React from 'react';
import { loadFromApi } from '../../api/ApiLoader';
import { useHistory } from 'react-router-dom';

export const LoginsInvite: React.FC<{ token: string, client: Client }> = (props) => {

  const [inviting, setInviting] = React.useState<boolean>(false);
  const [inviteDone, setInviteComplete] = React.useState<boolean>(false);
  const history = useHistory();

  const handleInviteSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const inputs = e.target as typeof e.target & {
      emailAddress: { value: string };
    };

    if (inputs.emailAddress.value.length === 0) {
      alert('Please enter an email address to invite');
      return;
    }

    setInviting(true);
    setInviteComplete(false);

    loadFromApi('Users/Invite', 'POST', props.token, { emailAddress: inputs.emailAddress.value }, (dataText: string) => {
      alert('Could not invite new user. Errors:\n' + dataText);
    })
      .then(() => {
        setInviteComplete(true);
        setTimeout(function () {
          setInviteComplete(false);
          setInviting(false);
          goToList();
        }, 3000);
      })
      .catch(() => {
        setInviting(false);
      });
  }

  const goToList = () => {
    history.push('/loginsList');
  }

  return (
    <div>
      <section className="page--header">
        <div className="page-title">
          <h1>Invite User</h1>
          <p>Invite a new user to the analytics.</p>
        </div>
      </section>

      <section className="profile--form nopad">
        <div className="form-wrap">
          <form onSubmit={handleInviteSubmit}>
            <div className="entry-item item-one-half item-first">
              <label>Email Address</label>
              <input type="text" id="emailAddress" name="emailAddress" title="Please enter the email address to invite" placeholder="Email Address" aria-label="Email Address" />
            </div>

            <div className="entry-item item-one-half item-first">
              <button type="submit" className="btn" disabled={inviting} style={{ marginRight: 10 }}>Send Invitation</button>
              <button onClick={goToList} className="btn light">Cancel</button>
              {inviteDone &&
                <span style={{ marginLeft: 20, color: 'green' }}>User invite has been sent.</span>
              }
            </div>
          </form>
        </div>
      </section>
    </div>
  );
};
