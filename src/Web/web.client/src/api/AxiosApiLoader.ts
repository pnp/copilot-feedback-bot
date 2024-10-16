import { loginRequest, teamsAppConfig } from "../authConfig";
import { AccountInfo } from "@azure/msal-common";
import { IPublicClientApplication } from "@azure/msal-browser";
import { BearerTokenAuthProvider, createApiClient, TeamsUserCredential } from "@microsoft/teamsfx";

import { AxiosInstance } from 'axios';

export abstract class BaseAxiosApiLoader {
    baseUrl: string;
    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }
    abstract logOut: () => void;

    createApiClient(baseUrl: string): AxiosInstance {
        // Create new instance of axios
        const axios = require('axios');

        return axios;
    }

    loadFromApi = async (url: string, method: string, body?: any, onError?: Function): Promise<string> => {
        const client = this.createApiClient(this.baseUrl);
        try {
            const response = await client.request({
                url,
                method,
                data: body
            });
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
        return createApiClient(
            baseUrl,
            new BearerTokenAuthProvider(async () => (await this._teamsUserCredential.getToken(loginRequest.scopes))!.token)
        );
    }

}
