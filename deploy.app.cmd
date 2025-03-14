:: Copyright (c) Microsoft Corporation.
:: Licensed under the MIT License.

@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

:: ----------------------
:: KUDU Deployment Script
:: Version: 1.0.0
:: ----------------------

:: Prerequisites
:: -------------

:: Verify node.js installed
where node 2>nul >nul
IF %ERRORLEVEL% NEQ 0 (
  echo Missing node.js executable, please install node.js, if already installed make sure it can be reached from current environment.
  goto error
)

:: Setup
:: -----

setlocal enabledelayedexpansion

SET ARTIFACTS=%~dp0%..\artifacts

IF NOT DEFINED DEPLOYMENT_SOURCE (
  SET DEPLOYMENT_SOURCE=%~dp0%.
)

IF NOT DEFINED DEPLOYMENT_TARGET (
  SET DEPLOYMENT_TARGET=%ARTIFACTS%\wwwroot
)

IF NOT DEFINED NEXT_MANIFEST_PATH (
  SET NEXT_MANIFEST_PATH=%ARTIFACTS%\manifest

  IF NOT DEFINED PREVIOUS_MANIFEST_PATH (
    SET PREVIOUS_MANIFEST_PATH=%ARTIFACTS%\manifest
  )
)

IF NOT DEFINED KUDU_SYNC_CMD (
  :: Install kudu sync
  echo Installing Kudu Sync
  call npm install kudusync -g --silent
  IF !ERRORLEVEL! NEQ 0 goto error

  :: Locally just running "kuduSync" would also work
  SET KUDU_SYNC_CMD=%appdata%\npm\kuduSync.cmd
)
IF NOT DEFINED DEPLOYMENT_TEMP (
  SET DEPLOYMENT_TEMP=%temp%\___deployTemp%random%
  SET CLEAN_LOCAL_DEPLOYMENT_TEMP=true
)
echo Deployment temp: %DEPLOYMENT_TEMP%

IF DEFINED CLEAN_LOCAL_DEPLOYMENT_TEMP (
  IF EXIST "%DEPLOYMENT_TEMP%" rd /s /q "%DEPLOYMENT_TEMP%"
  mkdir "%DEPLOYMENT_TEMP%"
)

IF DEFINED MSBUILD_PATH goto MsbuildPathDefined
SET MSBUILD_PATH=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe
:MsbuildPathDefined
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Deployment
:: ----------

echo Handling ASP.NET Core Web Application deployment.

:: Restore nuget packages
echo Restoring NuGet packages
call :ExecuteCmd dotnet restore "%DEPLOYMENT_SOURCE%\src\Copilot Feedback Bot.sln"
IF !ERRORLEVEL! NEQ 0 goto error

:: Build and publish console project. Clean the runtime folder
echo Building the console application
call :ExecuteCmd dotnet publish "%DEPLOYMENT_SOURCE%\src\ActivityImporter.ConsoleApp\ActivityImporter.ConsoleApp.csproj" --output "%DEPLOYMENT_TEMP%\app_data\Jobs\Triggered\ActivityImporter.ConsoleApp" --configuration Release -property:KuduDeployment=1

:: Create environment settings file
echo Creating environment settings file
echo off 
(
    echo VITE_MSAL_AUTHORITY=https://login.microsoftonline.com/organizations
    echo VITE_MSAL_CLIENT_ID=%ImportAuthConfig:ClientId%
    echo VITE_MSAL_SCOPES=%ImportAuthConfig:ApiScope%
    echo VITE_TEAMSFX_START_LOGIN_PAGE_URL=%WebRoot%/auth-start.html
) > "%DEPLOYMENT_SOURCE%\src\Web\web.client\.env.production"

:: Build and publish Web project
echo Building the web application
call :ExecuteCmd dotnet publish "%DEPLOYMENT_SOURCE%\src\Web\Web.Server\Web.Server.csproj" --output "%DEPLOYMENT_TEMP%" --configuration Release -property:KuduDeployment=1
IF !ERRORLEVEL! NEQ 0 goto error

:: KuduSync
echo Target dir: %DEPLOYMENT_TARGET%
call :ExecuteCmd "%KUDU_SYNC_CMD%" -v 50 -f "%DEPLOYMENT_TEMP%" -t "%DEPLOYMENT_TARGET%" -n "%NEXT_MANIFEST_PATH%" -p "%PREVIOUS_MANIFEST_PATH%" -i ".git;.hg;.deployment;deploy.cmd"
IF !ERRORLEVEL! NEQ 0 goto error

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
goto end

:: Execute command routine that will echo out when error
:ExecuteCmd
setlocal
set _CMD_=%*
call %_CMD_%
if "%ERRORLEVEL%" NEQ "0" echo Failed exitCode=%ERRORLEVEL%, command=%_CMD_%
exit /b %ERRORLEVEL%

:error
endlocal
echo An error has occurred during web site deployment.
call :exitSetErrorLevel
call :exitFromFunction 2>nul

:exitSetErrorLevel
exit /b 1

:exitFromFunction
()

:end
endlocal
echo Finished successfully.
