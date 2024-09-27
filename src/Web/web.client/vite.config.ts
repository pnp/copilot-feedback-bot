import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "web.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7095';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    optimizeDeps: {
        include: ['@mui/material/Tooltip', '@emotion/styled'],
    },
    server: {
        proxy: {
            "^/Users": {
                target,
                secure: false
            },
            "^/Clients": {
                target,
                secure: false
            },
            "^/Stats": {
                target,
                secure: false
            },
            "^/SkillsInsightsReports": {
                target,
                secure: false
            },
            "^/Lookups": {
                target,
                secure: false
            },
            "^/Charts": {
                target,
                secure: false
            },
            "^/Imports": {
                target,
                secure: false
            },
            "^/SkillsInitiatives": {
                target,
                secure: false
            },
            "^/ImportConfigurations": {
                target,
                secure: false
            },
            "^/Secrets": {
                target,
                secure: false
            },
            "^/Reports": {
                target,
                secure: false
            }
        },
        port: 5173,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    }
})
