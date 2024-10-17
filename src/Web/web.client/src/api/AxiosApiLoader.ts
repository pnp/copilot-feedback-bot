import { loginRequest } from "../authConfig";
import { AccountInfo } from "@azure/msal-common";
import { IPublicClientApplication } from "@azure/msal-browser";
import { AxiosInstance, BearerTokenAuthProvider, createApiClient, TeamsUserCredential } from "@microsoft/teamsfx";

export abstract class BaseAxiosApiLoader {
    baseUrl: string;
    client?: AxiosInstance;
    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }
    abstract logOut: () => void;

    abstract createApiClient(baseUrl: string): AxiosInstance;

    loadFromApi = async (url: string, method: string, body?: any, onError?: Function): Promise<any> => {

        if (!this.client) {
            this.client = this.createApiClient(this.baseUrl);
            console.debug("Client created");
        }
        console.debug(`Calling ${url} with method ${method} and body: `, body);
        try {
            const response = await this.client.request({
                url,
                method,
                data: body
            });

            console.log(`Response from ${url}: `, response.data);
            return response.data;
        } catch (err: unknown) {
            if (onError) {
                onError(err);
            }
            throw err;
        }
    };
}

export class TeamsSsoAxiosApiLoader extends BaseAxiosApiLoader {
    _teamsUserCredential: TeamsUserCredential;
    _client?: AxiosInstance;
    constructor(teamsUserCredential: TeamsUserCredential, baseUrl: string) {
        super(baseUrl);
        this._teamsUserCredential = teamsUserCredential;
    }

    logOut = () => {
    }

    override createApiClient(baseUrl: string): AxiosInstance {

        // Hack?
        console.log("Creating SSO API client with base URL: ", baseUrl);
        const c = createApiClient(
            baseUrl,
            new BearerTokenAuthProvider(async () => {
                console.log("Getting token from TeamsUserCredential");
                return (await this._teamsUserCredential.getToken(loginRequest.scopes))!.token;
            })
        );

        return c;
    }

}

export class MsalAxiosApiLoader extends BaseAxiosApiLoader {
    _publicClientApplication: IPublicClientApplication;
    _account: AccountInfo | null;
    _client?: AxiosInstance;
    constructor(publicClientApplication: IPublicClientApplication, account: AccountInfo | null, baseUrl: string) {
        super(baseUrl);

        console.log(`MsalAxiosApiLoader created with base URL '${baseUrl}' account: `, account);
        this._publicClientApplication = publicClientApplication;
        this._account = account;
    }

    logOut = () => {
        this._publicClientApplication.logout({
            account: this._account
        });
    }

    override createApiClient(baseUrl: string): AxiosInstance {
        if (!this._client) {
            this._client = createApiClient(
                baseUrl,
                new BearerTokenAuthProvider(async () => {
                    const response = await this._publicClientApplication.acquireTokenSilent({
                        account: this._account!,
                        scopes: loginRequest.scopes
                    });
                    return response.accessToken;
                })
            );
        }
        return this._client;
    }
}