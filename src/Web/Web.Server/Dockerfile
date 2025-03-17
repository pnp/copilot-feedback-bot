# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS buildjs
USER root
RUN apt-get update
RUN apt-get install curl -y
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs -y

# Set JS vars
FROM buildjs AS build
ARG BUILD_CONFIGURATION=Release
ARG VITE_MSAL_CLIENT_ID
ARG VITE_MSAL_AUTHORITY
ARG VITE_MSAL_SCOPES
ARG VITE_TEAMSFX_START_LOGIN_PAGE_URL

WORKDIR /src
COPY ["Web/Web.Server/Web.Server.csproj", "Web/Web.Server/"]
COPY ["Common.Engine/Common.Engine.csproj", "Common.Engine/"]
COPY ["Common.DataUtils/Common.DataUtils.csproj", "Common.DataUtils/"]
COPY ["Entities.DB/Entities.DB.csproj", "Entities.DB/"]
COPY ["Directory.Build.props", ""]
RUN dotnet restore "./Web/Web.Server/Web.Server.csproj"
COPY . .

WORKDIR "/src/Web/Web.Server"
RUN dotnet build "./Web.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Web.Server.dll"]
