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

    abstract createApiClient: (baseUrl: string) => AxiosInstance;

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
