import { loginRequest } from "../authConfig";
import { AccountInfo } from "@azure/msal-common";
import { IPublicClientApplication } from "@azure/msal-browser";

export abstract class BaseApiLoader {
    abstract getToken: () => Promise<string>;

    abstract logOut: () => void;

    loadFromApi = async (url: string, method: string, body?: any, onError?: Function): Promise<string> => {

        const token: string = await this.getToken();
        var req: any = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token,
            },
            body: null
        };
        if (body)
            req.body = JSON.stringify(body);

        console.info(`Loading ${url}...`);

        return fetch(url, req)
            .then(async response => {

                if (response.ok) {
                    const dataText: string = await response.text();
                    return Promise.resolve(dataText);
                }
                else {
                    const dataText: string = await response.text();
                    if (!onError) {
                        const errorTitle = `Error ${response.status} ${method}ing to/from API '${url}'`;

                        if (dataText !== "")
                            alert(`${errorTitle}: ${dataText}`)
                        else
                            alert(errorTitle);
                        
                    }
                    else
                        onError(dataText);
                    return Promise.reject(dataText);
                }
            });
    };
}

export class MsalApiLoader extends BaseApiLoader {
    instance: IPublicClientApplication;
    accounts: AccountInfo[]

    constructor(instance: IPublicClientApplication, accounts: AccountInfo[]) {
        super();
        this.instance = instance;
        this.accounts = accounts;
    }

    logOut = () => {
        this.instance.logoutRedirect({ postLogoutRedirectUri: "/" });
    }

    getToken: () => Promise<string> = async () => {

        const request = {
            ...loginRequest,
            account: this.accounts[0]
        };            
        console.debug("Requesting MSAL access token");

        return this.instance.acquireTokenSilent(request).then((response) => {
            console.debug(response);
            console.debug("Got token via cached account: " + response.accessToken);
            return response.accessToken;
        }).catch((error: any) => {
            console.log(error);
            return this.instance.acquireTokenPopup(request).then((response) => {
                console.debug(response);
                console.debug("Got token via popup: " + response.accessToken);
                return (response.accessToken);
            })
        });
    }
}
